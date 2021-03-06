﻿using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServiceTest
{
	[TestClass]
	public class ServiceTests
	{
		[TestMethod]
		public void Success()
		{
			int counter = 0; int pollInterval = 0; int batchSize = 10;
			int tasksToCreate = 10000;

			Action<FinishResult, IAssemblyData, CancellationTokenSource> onRun = (r, d, ts) =>
			{
				Assert.IsNotNull(d);
				Assert.IsNull(r.Exception);
				Assert.AreEqual(FinishStatus.Succes, r.Status);
				Assert.AreEqual(3, r.Result);
				Assert.AreEqual(r.Id, d.Id);
				Interlocked.Increment(ref counter);
			};
			Action<FinishResult, IAssemblyData, Exception> onCancel = (f, d, e) => Interlocked.Increment(ref counter);
			Action<FinishResult, IAssemblyData, Exception> onError = (f, d, e) => Interlocked.Increment(ref counter);
			ServiceFactory factory = new ServiceFactory(pollInterval, batchSize, onRun, onCancel, onError);

			factory.Service.Start();
			Guid[] tasksCreated = factory.Enqueue(tasksToCreate);
			Assert.AreEqual(tasksToCreate, tasksCreated.Length);

			Thread.Sleep(1000 * 10); //wait all
			Assert.AreEqual(tasksToCreate, counter);
		}

		[TestMethod]
		public void StartStop()
		{
			int counter = 0; int pollInterval = 0; int batchSize = 10;
			int tasksToCreate = 10000;

			Action<FinishResult, IAssemblyData, CancellationTokenSource> onRun = (r, d, ts) =>
			{
				Assert.IsNotNull(d);
				Assert.IsNull(r.Exception);
				Assert.AreEqual(FinishStatus.Succes, r.Status);
				Assert.AreEqual(3, r.Result);
				Assert.AreEqual(r.Id, d.Id);
				Interlocked.Increment(ref counter);
			};
			Action<FinishResult, IAssemblyData, Exception> onCancel = (f, d, e) => Interlocked.Increment(ref counter);
			Action<FinishResult, IAssemblyData, Exception> onError = (f, d, e) => Interlocked.Increment(ref counter);

			ServiceFactory factory = new ServiceFactory(pollInterval, batchSize, onRun, onCancel, onError);

			// Start for the first time
			factory.Service.Start();

			factory.Enqueue(5000);
			// Stoping here
			factory.Service.Stop();
			factory.Enqueue(5000);
			// Start again
			factory.Service.Start();

			Thread.Sleep(1000 * 10); //wait all
			Assert.AreEqual(tasksToCreate, counter);
		}

		[TestMethod]
		public void NotTouchedIFCanceledBeforeStart()
		{
			int counter = 0; int pollInterval = 0; int batchSize = 10;
			int tasksToCreate = 10000;

			Action<FinishResult> onJobCompleted = (r) => Interlocked.Increment(ref counter);
			Action<FinishResult, IAssemblyData, Exception> onCancel = (f, d, e) => Interlocked.Increment(ref counter);
			Action<FinishResult, IAssemblyData, CancellationTokenSource> onRun = (r, d, ts) => Interlocked.Increment(ref counter);
			Action<FinishResult, IAssemblyData, Exception> onError = (f, d, e) => Interlocked.Increment(ref counter);
			ServiceFactory factory = new ServiceFactory(onJobCompleted, pollInterval, batchSize, onRun, onCancel, onError);

			factory.TokenSource.Cancel();

			factory.Service.Start();

			Guid[] tasksCreated = factory.Enqueue(tasksToCreate);

			Assert.AreEqual(tasksToCreate, tasksCreated.Length);

			Assert.AreEqual(0, counter);
		}

		[TestMethod]
		public void GlobalCancel()
		{
			int counter = 0; int pollInterval = 0; int batchSize = 10;
			int tasksToCreate = 10000;
			int cancelAfterMs = 5000;
			CancellationTokenSource globalTs = new CancellationTokenSource(cancelAfterMs);

			Action<FinishResult, IAssemblyData, Exception> onCancel = (f, d, e) => Interlocked.Increment(ref counter);
			Action<FinishResult, IAssemblyData, CancellationTokenSource> onRun = (r, d, ts) =>
			{
				Assert.IsNotNull(d);
				Assert.IsNull(r.Exception);
				Assert.AreEqual(FinishStatus.Succes, r.Status);
				Assert.AreEqual(3, r.Result);
				Assert.AreEqual(r.Id, d.Id);
				Interlocked.Increment(ref counter);
				Thread.Sleep(1000 * 60 * 60);
			};
			Action<FinishResult, IAssemblyData, Exception> onError = (f, d, e) => Interlocked.Increment(ref counter);
			ServiceFactory factory = new ServiceFactory(globalTs, pollInterval, batchSize, onRun, onCancel, onError);

			Guid[] tasksCreated = factory.Enqueue(tasksToCreate);  // takes about 3 seconds to add 10,000 on my laptop
			factory.Service.Start();
			Thread.Sleep(cancelAfterMs + 1000);                     // let it run for wait time to cancell + 1 sec

			Assert.AreEqual(tasksToCreate, tasksCreated.Length);

			Assert.IsTrue(counter > 0);                             // at least some should be started by now
			Assert.IsTrue(counter < tasksToCreate);                 // not all should be started by now
		}

		[TestMethod]
		public void CancellAtItemLevel()
		{
			int counter = 0; int pollInterval = 0; int batchSize = 10000;
			int tasksToCreate = 10000;

			Action<FinishResult, IAssemblyData, Exception> onCancel = (f, d, e) =>
			{
				Assert.IsNotNull(e);
				Assert.IsNotNull(f.Exception);
				Assert.AreEqual(FinishStatus.Canceled, f.Status);
				Assert.IsNull(f.Result);
				Assert.AreEqual(d.Id, f.Id);
				Interlocked.Increment(ref counter);
			};
			Action<FinishResult, IAssemblyData, CancellationTokenSource> onRun = (r, d, ts) =>
			{
				ts.Cancel();
			};
			Action<FinishResult, IAssemblyData, Exception> onError = (f, d, e) => Interlocked.Increment(ref counter);
			ServiceFactory factory = new ServiceFactory(pollInterval, batchSize, onRun, onCancel, onError);

			factory.Service.Start();

			Guid[] tasksCreated = factory.Enqueue(tasksToCreate);

			Thread.Sleep(2000); //wait for batch to start

			Assert.AreEqual(tasksToCreate, tasksCreated.Length);

			Assert.AreEqual(tasksToCreate, counter);
		}

		[TestMethod]
		public void TimeOutTasks()
		{
			int counter = 0; int pollInterval = 0; int batchSize = 10000;
			int tasksToCreate = 10000;
			int timeoutms = 10;

			Action<FinishResult, IAssemblyData, Exception> onCancel = (f, d, e) => Interlocked.Increment(ref counter);
			Action<FinishResult, IAssemblyData, CancellationTokenSource> onRun = (r, d, ts) =>
			{
				Assert.IsNotNull(d);
				Assert.IsNull(r.Exception);
				Assert.AreEqual(FinishStatus.Succes, r.Status);
				Assert.AreEqual(3, r.Result);
				Assert.AreEqual(r.Id, d.Id);
				Thread.Sleep(1000 * 60 * 60); //wait one hour for cancelation
			};
			Action<FinishResult, IAssemblyData, Exception> onError = (f, d, e) =>
			{
				Assert.IsNotNull(e);
				Assert.IsNotNull(f.Exception);
				Assert.AreEqual(FinishStatus.Error, f.Status);
				Assert.IsNull(f.Result);
				Assert.AreEqual(d.Id, f.Id);
				Interlocked.Increment(ref counter);
			};
			ServiceFactory factory = new ServiceFactory(pollInterval, batchSize, onRun, onCancel, onError);

			factory.Service.Start();

			Guid[] tasksCreated = factory.Enqueue(tasksToCreate, timeoutms);

			Thread.Sleep(2000); //wait for batch to start

			Assert.AreEqual(tasksToCreate, tasksCreated.Length);

			Assert.AreEqual(tasksToCreate, counter);
		}

		[TestMethod]
		public void ErroredTasks()
		{
			int counter = 0; int pollInterval = 0; int batchSize = 10;
			int tasksToCreate = 10;

			Action<FinishResult, IAssemblyData, Exception> onCancel = (f, d, e) => Interlocked.Increment(ref counter);
			Action<FinishResult, IAssemblyData, CancellationTokenSource> onRun = (r, d, ts) =>
			{
				Assert.IsNotNull(d);
				Assert.IsNull(r.Exception);
				Assert.AreEqual(FinishStatus.Succes, r.Status);
				Assert.AreEqual(3, r.Result);
				Assert.AreEqual(r.Id, d.Id);
				throw new Exception("Application Exception");
			};
			Action<FinishResult, IAssemblyData, Exception> onError = (f, d, e) =>
			{
				Assert.IsNotNull(e);
				Assert.IsNotNull(f.Exception);
				Assert.AreEqual(FinishStatus.Error, f.Status);
				Assert.IsNull(f.Result);
				Assert.AreEqual(d.Id, f.Id);
				Interlocked.Increment(ref counter);
			};
			ServiceFactory factory = new ServiceFactory(pollInterval, batchSize, onRun, onCancel, onError);

			factory.Service.Start();

			Guid[] tasksCreated = factory.Enqueue(tasksToCreate);

			Thread.Sleep(1000 * 10); //wait for batch to start

			Assert.AreEqual(tasksToCreate, tasksCreated.Length);

			Assert.AreEqual(tasksToCreate, counter);
		}
	}
}
