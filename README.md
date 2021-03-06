# Queue of Tasks
This is the implementation of a client/server model for processing generic tasks in multithreaded fashion.

It is implemented using .Net, and it is divided into two Visual Studio solutions:
  - /client/Client.sln
  - /server/Server.sln

### Client Part
It creates an executable console application that allows user to insert sample tasks to be executed later for the server application. Tasks will be enqueued in an existing table (see /DBStructure folder).

### Server Part
It contains a console application that dequeues and executes tasks concurrently from the database (other sources are also possible described later on ITaskQueue topic).

## Tasks Components
Here are the main components.

  - TaskRunner. It is an Fsharp wrapper around MailboxProcessor class that is the core part of the Server. It takes tasks in the form of ITask interface and executes them concurrently. 
  
    **Important:** Only one instance of the Server per ITaskQueue can run at a given time. You can run multiple instances as long as they have different queues. Client app on the other hand can have as many instances as you want.
  
  - ITask. It consists of three methods Run, OnError, OnCancel. Run is the code that will be executed by TaskRunner, OnError and OnCancel will be called if required.
  
  - ITaskQueue is the source of tasks. It can be anything. You will see two examples of it.
    - 1) DbQueue.cs. Used in this example. Takes data from Db.
    - 2) TestQueue.cs. Used in unit tests for the Server part. Takes data from collections under System.Collections.Concurrent namespace.
