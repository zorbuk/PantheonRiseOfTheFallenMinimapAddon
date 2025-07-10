using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PantheonRiseOfTheFallenMinimapAddon.marker
{
    public class MarkerManager
    {
        private readonly string _directory;
        private string _currentFileName;

        public string CurrentFilePath => Path.Combine(_directory, _currentFileName);
        public string CurrentFileName => _currentFileName;

        public MarkerManager(string directory, string defaultFileName)
        {
            if (string.IsNullOrWhiteSpace(directory))
                throw new ArgumentException("Directory cannot be null or empty.", nameof(directory));

            if (string.IsNullOrWhiteSpace(defaultFileName))
                throw new ArgumentException("Default file name cannot be null or empty.", nameof(defaultFileName));

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                if (defaultFileName.Contains(c))
                    throw new ArgumentException("Default file name contains invalid characters.", nameof(defaultFileName));
            }

            if (!defaultFileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                defaultFileName += ".json";

            _directory = directory;
            _currentFileName = defaultFileName;

            Directory.CreateDirectory(_directory);
        }

        public List<string> GetAvailableMarkerFiles()
        {
            return Directory.GetFiles(_directory, "*.json")
                            .Select(Path.GetFileName)
                            .Where(name => !string.IsNullOrWhiteSpace(name))
                            .ToList();
        }

        public void ChangeCurrentFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                if (fileName.Contains(c))
                    throw new ArgumentException("File name contains invalid characters.", nameof(fileName));
            }

            if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                fileName += ".json";

            string fullPath = Path.Combine(_directory, fileName);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException("Specified marker file does not exist.", fullPath);

            _currentFileName = fileName;
        }

        public bool RenameCurrentFile(string newFileName)
        {
            if (string.IsNullOrWhiteSpace(newFileName))
                return false;

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                if (newFileName.Contains(c))
                    return false;
            }

            if (!newFileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                newFileName += ".json";

            string oldPath = Path.Combine(_directory, _currentFileName);
            string newPath = Path.Combine(_directory, newFileName);

            if (!File.Exists(oldPath) || File.Exists(newPath))
                return false;

            try
            {
                File.Move(oldPath, newPath);
                _currentFileName = newFileName;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CreateNewMarkerFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                if (fileName.Contains(c))
                    return false;
            }

            if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                fileName += ".json";

            string fullPath = Path.Combine(_directory, fileName);
            if (File.Exists(fullPath))
                return false;

            try
            {
                File.WriteAllText(fullPath, "[]");
                _currentFileName = fileName;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
