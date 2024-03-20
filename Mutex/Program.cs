using Mutexes;

namespace Mutexes
{
    public class Reporter
    {
        private Mutex mutex = new Mutex();

        private string task1Path = "Random Numbers (1).txt";
        private string task2Path = "Prime Numbers (2).txt";
        private string task3Path = "Prime Numbers Ending With 7 (3).txt";

        private string reportPath = "Report.txt";

        public void Start()
        {
            CheckAndCreateTaskFiles();

            CreateAndStartTaskThreads();
        }

        private void CheckAndCreateTaskFiles()
        {
            List<string> paths = new List<string> {
            task1Path, task2Path, task3Path, reportPath
        };

            foreach (var path in paths)
                CheckAndCreateFile(path);
        }

        private void CreateAndStartTaskThreads()
        {
            List<Thread> threads = new List<Thread>
        {
            new Thread(GenerateRandomNumbers),
            new Thread(FindPrimes),
            new Thread(FindPrimesEndingWith7),
            new Thread(GenerateReport)
        };

            foreach (var thread in threads)
                thread.Start();

            foreach (var thread in threads)
                thread.Join();
        }

        private void GenerateRandomNumbers()
        {
            Random rand = new Random();

            List<int> numbers = new List<int>();
            for (int i = 0; i < 1000; i++)
            {
                numbers.Add(rand.Next(1, 1000));
            }

            mutex.WaitOne();
            File.WriteAllLines(task1Path, numbers.Select(num => num.ToString()).ToArray());
            mutex.ReleaseMutex();
        }

        private void FindPrimes()
        {
            mutex.WaitOne();
            var numbers = File.ReadAllLines(task1Path).Select(int.Parse).ToList();
            mutex.ReleaseMutex();

            var primes = numbers.Where(IsPrime).ToList();

            mutex.WaitOne();
            File.WriteAllLines(task2Path, primes.Select(num => num.ToString()).ToArray());
            mutex.ReleaseMutex();
        }

        private void FindPrimesEndingWith7()
        {
            mutex.WaitOne();
            var primes = File.ReadAllLines(task2Path).Select(int.Parse).ToList();
            mutex.ReleaseMutex();

            var primesEndingIn7 = primes.Where(num => num.ToString().Last().Equals('7')).ToList();

            mutex.WaitOne();
            File.WriteAllLines(task3Path, primesEndingIn7.Select(num => num.ToString()).ToArray());
            mutex.ReleaseMutex();
        }

        private void GenerateReport()
        {
            mutex.WaitOne();
            var file1 = File.ReadAllLines(task1Path);
            var file2 = File.ReadAllLines(task2Path);
            var file3 = File.ReadAllLines(task3Path);
            mutex.ReleaseMutex();

            var file1Info = new FileInfo(task1Path);
            var file2Info = new FileInfo(task2Path);
            var file3Info = new FileInfo(task3Path);

            using (StreamWriter sw = new StreamWriter(reportPath))
            {
                sw.WriteLine($"{file1Info.Name}: {file1.Length} numbers, {file1Info.Length} bytes");
                sw.WriteLine(string.Join(", ", file1) + "\n");

                sw.WriteLine($"{file2Info.Name}: {file2.Length} numbers, {file2Info.Length} bytes");
                sw.WriteLine(string.Join(", ", file2) + "\n");

                sw.WriteLine($"{file3Info.Name}: {file3.Length} numbers, {file3Info.Length} bytes");
                sw.WriteLine(string.Join(", ", file3) + "\n");
            }
        }

        private void CheckAndCreateFile(string path)
        {
            if (!File.Exists(path))
                using (FileStream fs = File.Create(path)) { }
        }

        private bool IsPrime(int number)
        {
            if (number <= 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            var boundary = (int)Math.Floor(Math.Sqrt(number));

            for (int i = 3; i <= boundary; i += 2)
                if (number % i == 0)
                    return false;
            return true;
        }
    }
}
class Program
{
    static void Main(string[] args)
    {
        Reporter reporter = new Reporter();
        reporter.Start();
    }
}