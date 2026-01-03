using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TaskTracker
{
    internal enum TaskStatus { TODO, INPROGRESS, DONE }
    internal class Task
    {

        private static Dictionary<int, string> StatusMap = new() {
            { 0, "to-do" },
            { 1, "in-progress" },
            { 2, "done" },
        };
        public static readonly string TASKSPATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "taskscli");

        public readonly string id;
        private string _name;
        private string _description;
        private TaskStatus _status = TaskStatus.TODO;
        private string _updated_at = DateTime.Now.ToString();
        private string _created_at = DateTime.Now.ToString();

        public string Description { 
            get { return _description; } 
            set { 
                _description = value;
                _updated_at = DateTime.Now.ToString();
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                _updated_at = DateTime.Now.ToString();
            }
        }

        public TaskStatus Status
        {
            get { return _status; }
            set
            {
                if (!Enum.IsDefined(typeof(TaskStatus), (int)value))
                {
                    throw new Exception("New value of status should be valid!");
                }
                _status = value;
                _updated_at = DateTime.Now.ToString();
            }
        }

        override public string ToString()
        {
            return $"{id}, {Name}, {StatusMap[(int)Status]}, {_created_at}, {_updated_at}, {Description}";
        }

        public Task(string name, string description) {
            _name = (String.IsNullOrWhiteSpace(name)) ? "no name" : name.Trim();
            _description = (String.IsNullOrWhiteSpace(description)) ? "no desc" : description.Trim();

            string SourceData = _name + _description + _created_at;
            byte[] tmpSource = UTF8Encoding.UTF8.GetBytes(SourceData);
            byte[] tmpHash = MD5.Create().ComputeHash(tmpSource);

            StringBuilder sb = new();
            foreach (byte b in tmpHash) {
                sb.Append(b.ToString("x2"));
            }

            id = sb.ToString();
        }

        private Task(string id) { this.id = id; }

        public static Task FromJson(string path)
        {
            using var stream = File.OpenRead(path);
            using var document = JsonDocument.Parse(stream);
            
            JsonElement root = document.RootElement;
            string id = root.GetProperty("Id").GetString() ?? "";
            string name = root.GetProperty("Name").GetString() ?? "";
            int status = root.GetProperty("Status").GetInt32();
            string description = root.GetProperty("Description").GetString() ?? "";
            string created_at = root.GetProperty("Created_at").GetString() ?? "";
            string updated_at = root.GetProperty("Updated_at").GetString() ?? "";

            string[] fields = { id, name, description, created_at, updated_at };

            if (fields.Any(string.IsNullOrWhiteSpace))
            {
                throw new InvalidOperationException("One or more required fields are missing or empty. Broken JSON.");
            }
            if (!DateTime.TryParse(created_at, out _) || !DateTime.TryParse(updated_at, out _))
            {
                throw new InvalidOperationException("Creating or Update times are incorrect. Broken JSON.");
            }
            if (!Enum.IsDefined(typeof(TaskStatus), status))
            {
                throw new InvalidOperationException("Status propery is incorrect for Task. Broken JSON.");
            }

            return new Task(id) { 
                _name = name,
                _description = description,
                _status = (TaskStatus)status,
                _created_at = created_at,
                _updated_at = updated_at,
            };
        }

        public void ToJson()
        {
            var options = new JsonWriterOptions { Indented = true };
            if (!Directory.Exists(TASKSPATH))
            {
                Directory.CreateDirectory(TASKSPATH);
            }

            using (var stream = File.Create(Path.Combine(TASKSPATH, id + ".json")))
            using (var writer = new Utf8JsonWriter(stream, options))
            {
                writer.WriteStartObject();
                writer.WriteString("Id", id);
                writer.WriteString("Name", Name);
                writer.WritePropertyName("Status");
                writer.WriteNumberValue(((int)_status));
                writer.WriteString("Description", Description);
                writer.WriteString("Created_at", _created_at);
                writer.WriteString("Updated_at", _updated_at);
                writer.WriteEndObject();
            }
        }
    }
}
