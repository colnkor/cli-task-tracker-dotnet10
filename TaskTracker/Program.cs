
using System.Text.Json;

namespace Program
{
    class Program
    {
        static public void CreateTask(string name, string description)
        {
            TaskTracker.Task aTask = new(name, description);
            aTask.ToJson();
            Console.WriteLine($"New task file created at: {TaskTracker.Task.TASKSPATH}");
            Console.WriteLine($"Task id: {aTask.id}");
        }

        static public TaskTracker.Task FindTask(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                throw new Exception("ID cannot be empty!");
            }

            return TaskTracker.Task.FromJson(Path.Combine(TaskTracker.Task.TASKSPATH, id + ".json"));
        }
        
        static public void RemoveTask(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                throw new Exception("ID cannot be empty!");
            }

            File.Delete(Path.Combine(TaskTracker.Task.TASKSPATH, id + ".json"));
            Console.WriteLine("Removed.");
        }

        static public void UpdateTask(string id, string description)
        {
            if (String.IsNullOrWhiteSpace(description))
            {
                throw new Exception("Description cannot be empty!");
            }

            var task = FindTask(id);
            task.Description = description;
            task.ToJson();
            Console.WriteLine($"Task (id: {task.id}) has been updated.\n{task}");
        }

        static public void MarkTask(string id, TaskTracker.TaskStatus newStatus) 
        {
            if (!Enum.IsDefined(typeof(TaskTracker.TaskStatus), (int)newStatus))
            {
                throw new Exception("New value of status should be valid!");
            }
            var task = FindTask(id);
            task.Status = newStatus;
            task.ToJson();
        }

        static public void ShowTask(string id)
        {
            var aTask = FindTask(id);
            Console.WriteLine(aTask);
        }

        static public void ListTasks(string? option = null)
        {
            var status_filter = option switch
            {
                "done" => (TaskTracker.TaskStatus.DONE),
                "todo" => (TaskTracker.TaskStatus.TODO),
                "in-progress" => (TaskTracker.TaskStatus.INPROGRESS),
                _ => (TaskTracker.TaskStatus)(-1)
            };

            string[] files = Directory.GetFiles(TaskTracker.Task.TASKSPATH);

            Console.WriteLine("id, name, status, created_at, updated_at, description");
            foreach (string file in files)
            {
                var task = TaskTracker.Task.FromJson(file);
                if ((int)status_filter == -1 || status_filter == task.Status)
                {
                    Console.WriteLine(task);
                }
            }
            Console.WriteLine("+++++++++++++++++++++++++");
        }

        static public void Main(string[] args)
        {
            string command = args.Length > 0 ? args[0] : "help";

            try
            {
                switch (command)
                {
                    case "add" when args.Length == 3:
                        CreateTask(args[1], args[2]);
                        break;
                    case "update" when args.Length == 3:
                        UpdateTask(args[1], args[2]);
                        break;
                    case "remove" when args.Length == 2:
                    case "delete" when args.Length == 2:
                        RemoveTask(args[1]);
                        break;
                    case "show" when args.Length == 2:
                        ShowTask(args[1]);
                        break;
                    case "mark-todo" when args.Length == 2:
                        MarkTask(args[1], TaskTracker.TaskStatus.TODO);
                        break;
                    case "mark-in-progress" when args.Length == 2:
                        MarkTask(args[1], TaskTracker.TaskStatus.INPROGRESS);
                        break;
                    case "mark-done" when args.Length == 2:
                        MarkTask(args[1], TaskTracker.TaskStatus.DONE);
                        break;
                    case "get-curr-folder":
                        Console.WriteLine(TaskTracker.Task.TASKSPATH);
                        break;
                    case "list":
                        ListTasks((args.Length > 1) ? args[1] : null);
                        break;
                    case "help":
                    default:
                        Console.WriteLine("""
                            AVAILABLE COMMANDS
                                add [name] [description]                - create new task with name & description
                                update [id] [description]               - update task by id
                                delete [id]                             - delete task by id
                                show [id]                               - show task by id
                                mark-todo [id]                          - mark todo
                                mark-in-progress [id]                   - mark in progress by id
                                mark-done [id]                          - mark done by id
                                list [?type: done | todo | in-progress] - list all tasks
                                get-curr-folder                         - current working directory
                            """);
                        break;
                }
            }
            catch (JsonException jex)
            {
                Console.WriteLine(jex.Message);
                Console.WriteLine("It seems, that json file of task is corrupted! Oh uh...");
            }
            catch (InvalidOperationException ipex)
            {
                Console.WriteLine(ipex.Message);
                Console.WriteLine("Something went wrong while parsing task properties");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Write help command to show all commands of CLI tool");
            }
        }
    }
}