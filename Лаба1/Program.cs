using System;
using System.Linq;
using System.IO;

namespace Лаба1
{
    public class Program
    {
        private static string savePath = Environment.ProcessPath.Substring(0, Environment.ProcessPath.LastIndexOf('\\')) + "\\Processes.txt"; //Путь до файла с сохранением

        static void Main(string[] args)
        {
            MakeFCFSProcess(GetTasksFromFile());//Запускаем процессор FCFS
            MakeRRProcess(GetTasksFromFile());//Запускаем процессор RR
            MakeSJFProcess(GetTasksFromFile());//Запускаем процеесор SJF
            MakeRProcess(GetTasksFromFile());//Запускаем процессор свой вариант

            Console.ReadKey();
        }

        private static List<SystemTask> GetTasksFromFile()
        {
            List<SystemTask> result = new List<SystemTask>();//Результат

            if (!File.Exists(savePath))//Проверяем создан ли файл
                LoadDefaultSaveFile();//Если нет, то создаём по умолчанию

            foreach (var item in File.ReadAllLines(savePath))//Перечисляем каждую строку файла
                result.Add(SystemTask.Parse(item));//Превращаем строку в SystemTask

            if (result.Count==0)//Если в итоге не создались задачи, то загружаем по умолчанию
            {
                LoadDefaultSaveFile();//Загружаем файл по умолчанию
                return GetTasksFromFile();//Рекурсия
            }

            return result;//Вовзращаем результат
        }

        private static void LoadDefaultSaveFile()//Загрузка файла по умолчанию
        {
            if (File.Exists(savePath))//Если файл есть, то удаляем
                File.Delete(savePath);

            File.Create(savePath).Close();//Создаём файл и закрываем поток (чтобы можно было редактировать дальше)

            List<string> lines = new List<string>() {//Список строк, которые будут записаны в файл
            "p0|6|0|0",
            "p1|2|2|0",
            "p2|7|6|0",
            "p3|5|0|0",
            };

            File.WriteAllLines(savePath, lines);//Записываем строки в файл
        }

        private static void MakeFCFSProcess(List<SystemTask> tasks)
        {
            FCFSProcessor processor = new FCFSProcessor(tasks);//Создаём процессор

            processor.StartProcess();//Запускаем

            Console.WriteLine("FCFS:");
            foreach (var item in tasks)//Выводим все обработанные задачи
                Console.WriteLine(item.ToString());
            Console.WriteLine($"Эффектикность: {processor.Efficiency}");//Выводим эффективность
        }

        private static void MakeRRProcess(List<SystemTask> tasks)
        {
            RRProcessor processor = new RRProcessor(tasks, 1);

            processor.StartProcess();

            Console.WriteLine("\n\nRR:");
            foreach (var item in tasks)
                Console.WriteLine(item.ToString());
            Console.WriteLine($"Эффектикность: {processor.Efficiency}");
        }

        private static void MakeSJFProcess(List<SystemTask> tasks)
        {
            SJFProcessor processor = new SJFProcessor(tasks);

            processor.StartProcess();

            Console.WriteLine("\n\nSJF:");
            foreach (var item in tasks)
                Console.WriteLine(item.ToString());
            Console.WriteLine($"Эффектикность: {processor.Efficiency}");
        }

        private static void MakeRProcess(List<SystemTask> tasks)
        {
            RProcessor processor = new RProcessor(tasks);

            processor.StartProcess();

            Console.WriteLine("\n\nСвой вариант:");
            foreach (var item in tasks)
                Console.WriteLine(item.ToString());
            Console.WriteLine($"Эффектикность: {processor.Efficiency}");
        }
    }

    public class SystemTask
    {
        private int startTimeDelay = 0;//Начальное время задержки
        private int timeDelay = 0;//Оставщееся время задержки
        private int priority = 0;//Приоритет
        private int startExecuteTimes = 0;//Начальное количество выполнений
        private int executeTimes = 0;//Количество выполнений

        private string outString = "";
        private string name;

        public int TimeDelay { get { return timeDelay; } }
        public int StartTimeDelay { get { return startTimeDelay; } }
        public int Priority { get { return priority; } }
        public int ExecuteTimes { get { return executeTimes; } }

        public SystemTask(string Name, int ExecuteTimes, int TimeDelay = 0, int Priority = 0)
        {
            name=Name;
            timeDelay = TimeDelay;
            startTimeDelay = TimeDelay;
            priority = Priority;
            executeTimes = ExecuteTimes;
            startExecuteTimes = ExecuteTimes;
        }

        public bool IsValid()//Актуальна ли задача
        {
            return timeDelay==0 && executeTimes>0;
        }

        public void Wait()//Вызывается при выполнении других задач
        {
            if (!IsValid())
                return;

            outString+="Г ";
        }

        public void Execute()//Вызывается при выполнении этой задачи
        {
            if (!IsValid())
                return;

            outString+="И ";
            executeTimes--;
        }

        public void TimeDecrease()//Вызывается при ожидании задачи
        {
            timeDelay -=1;

            if (timeDelay<0)
                timeDelay = 0;
            else
                outString += "  ";
        }

        public override string ToString()
        {
            return $"{name} | {startExecuteTimes} | {priority} | {startTimeDelay} | {outString}";
        }

        public static SystemTask Parse(string item)
        {
            string[] args = item.Split('|');

            return new SystemTask(args[0], int.Parse(args[1]), int.Parse(args[2]), int.Parse(args[3]));
        }
    }

    public class SJFProcessor
    {
        private List<SystemTask> allTasks = new List<SystemTask>();//Все задачи

        private List<SystemTask> currentExecutionTasks = new List<SystemTask>();//Текущие задачи

        private int totalWaiters = 0;
        private int totalOperations = 0;

        public float Efficiency { get { return (float)totalWaiters/totalOperations; } }

        public SJFProcessor(List<SystemTask> AllTasks)
        {
            allTasks = new List<SystemTask>(AllTasks);
        }

        public void StartProcess()//Начать выполнение процесса
        {
            UpdateCurrentExecutionTasks();//Обновляем текущие задачи (удаляем ненужные + сортируем)

            while (currentExecutionTasks.Count != 0)//Пока есть текущие задачи
            {
                for (int i = 0; i<currentExecutionTasks.Count; i++)//Перечисляем отсортированные текущие задачи
                {
                    if (i==0)
                        currentExecutionTasks[i].Execute();//Выполняем первую текущую задачу
                    else
                    {
                        currentExecutionTasks[i].Wait();//Остальные задачи ожидают
                        totalWaiters++;
                    }

                    totalOperations++;
                }

                UpdateCurrentExecutionTasks();//Обновляем текущие задачи (удаляем ненужные + сортируем)
            }
        }

        private void UpdateCurrentExecutionTasks()//Обновление и сортировка текущих задач
        {
            foreach (var item in allTasks)//Перечиляем все задачи
            {
                if (currentExecutionTasks.Contains(item))//Если эта задача есть в текущих, то пропускаем
                    continue;

                if (item.IsValid())//Если задача готова к началу работы, то добавляем в текущие
                    currentExecutionTasks.Add(item);
                else if (item.TimeDelay>0)//Если не готова, то уменьшаем ей задержку до выполнения
                    item.TimeDecrease();
            }

            List<SystemTask> toRemove = new List<SystemTask>();//Задачи к удалению из текущих

            foreach (var item in currentExecutionTasks)//Перечисляем текущие задачи
                if (!item.IsValid())//Если задача уже не акатуальна, то добавляем на удаление
                    toRemove.Add(item);

            foreach (var item in toRemove)//Перечисляем все задачи для удаления
                currentExecutionTasks.Remove(item);//Удаляем задачу

            currentExecutionTasks = currentExecutionTasks.OrderBy(x => x.ExecuteTimes).ToList();//Сортируем задачи по наименьшему количеству выполнений

            if (currentExecutionTasks.Count>1)//Если количество задач больше, чем 1, то сортируем по приоритету
                currentExecutionTasks = currentExecutionTasks.OrderBy(x => x.Priority).ToList();//Сортируем по приоритету
        }
    }

    public class FCFSProcessor
    {
        private List<SystemTask> allTasks = new List<SystemTask>();//Все задачи

        private List<SystemTask> currentExecutionTasks = new List<SystemTask>();//Текущие задачи

        private int totalWaiters = 0;
        private int totalOperations = 0;

        public float Efficiency { get { return (float)totalWaiters/totalOperations; } }

        public FCFSProcessor(List<SystemTask> AllTasks)
        {
            allTasks = new List<SystemTask>(AllTasks);
        }

        public void StartProcess()//Начать выполнение процесса
        {
            UpdateCurrentExecutionTasks();//Обновляем текущие задачи (удаляем ненужные + сортируем)

            while (currentExecutionTasks.Count != 0)//Пока есть текущие задачи
            {
                for (int i = 0; i<currentExecutionTasks.Count; i++)//Перечисляем отсортированные текущие задачи
                {
                    if (i==0)
                        currentExecutionTasks[i].Execute();//Выполняем первую текущую задачу
                    else
                    {
                        currentExecutionTasks[i].Wait();//Остальные задачи ожидают
                        totalWaiters++;
                    }

                    totalOperations++;
                }

                UpdateCurrentExecutionTasks();//Обновляем текущие задачи (удаляем ненужные + сортируем)
            }
        }

        private void UpdateCurrentExecutionTasks()//Обновление и сортировка текущих задач
        {
            foreach (var item in allTasks)//Перечиляем все задачи
            {
                if (currentExecutionTasks.Contains(item))//Если эта задача есть в текущих, то пропускаем
                    continue;

                if (item.IsValid())//Если задача готова к началу работы, то добавляем в текущие
                    currentExecutionTasks.Add(item);
                else if (item.TimeDelay>0)//Если не готова, то уменьшаем ей задержку до выполнения
                    item.TimeDecrease();
            }

            List<SystemTask> toRemove = new List<SystemTask>();//Задачи к удалению из текущих

            foreach (var item in currentExecutionTasks)//Перечисляем текущие задачи
                if (!item.IsValid())//Если задача уже не акатуальна, то добавляем на удаление
                    toRemove.Add(item);

            foreach (var item in toRemove)//Перечисляем все задачи для удаления
                currentExecutionTasks.Remove(item);//Удаляем задачу
        }
    }

    public class RRProcessor
    {
        private List<SystemTask> allTasks = new List<SystemTask>();//Все задачи

        private List<SystemTask> currentExecutionTasks = new List<SystemTask>();//Текущие задачи

        private int totalWaiters = 0;
        private int totalOperations = 0;

        public float Efficiency { get { return (float)totalWaiters/totalOperations; } }

        private int maxStep;
        private int lastIndex = 0;
        private int lastStep = 0;

        public RRProcessor(List<SystemTask> AllTasks, int step)
        {
            allTasks = new List<SystemTask>(AllTasks);
            maxStep = step;
        }

        public void StartProcess()//Начать выполнение процесса
        {
            UpdateCurrentExecutionTasks();//Обновляем текущие задачи (удаляем ненужные + сортируем)

            while (currentExecutionTasks.Count != 0)//Пока есть текущие задачи
            {
                if (lastStep==maxStep)
                {
                    lastStep = 0;
                    lastIndex++;
                }

                if (lastIndex>=currentExecutionTasks.Count)
                    lastIndex = 0;

                for (int i = 0; i<currentExecutionTasks.Count; i++)//Перечисляем отсортированные текущие задачи
                {
                    if (i==lastIndex)
                    {
                        currentExecutionTasks[i].Execute();//Выполняем первую текущую задачу
                        lastStep++;
                    }
                    else
                    {
                        currentExecutionTasks[i].Wait();//Остальные задачи ожидают
                        totalWaiters++;
                    }

                    totalOperations++;
                }

                if (!currentExecutionTasks[lastIndex].IsValid())
                {
                    lastStep=0;
                    lastIndex++;
                }

                UpdateCurrentExecutionTasks();//Обновляем текущие задачи (удаляем ненужные + сортируем)
            }
        }

        private void UpdateCurrentExecutionTasks()//Обновление и сортировка текущих задач
        {
            foreach (var item in allTasks)//Перечиляем все задачи
            {
                if (currentExecutionTasks.Contains(item))//Если эта задача есть в текущих, то пропускаем
                    continue;

                if (item.IsValid())//Если задача готова к началу работы, то добавляем в текущие
                    currentExecutionTasks.Add(item);
                else if (item.TimeDelay>0)//Если не готова, то уменьшаем ей задержку до выполнения
                    item.TimeDecrease();
            }

            List<SystemTask> toRemove = new List<SystemTask>();//Задачи к удалению из текущих

            foreach (var item in currentExecutionTasks)//Перечисляем текущие задачи
                if (!item.IsValid())//Если задача уже не акатуальна, то добавляем на удаление
                    toRemove.Add(item);

            foreach (var item in toRemove)//Перечисляем все задачи для удаления
                currentExecutionTasks.Remove(item);//Удаляем задачу
        }
    }

    public class RProcessor
    {
        private List<SystemTask> allTasks = new List<SystemTask>();//Все задачи

        private List<SystemTask> currentExecutionTasks = new List<SystemTask>();//Текущие задачи

        private int totalWaiters = 0;
        private int totalOperations = 0;

        public float Efficiency { get { return (float)totalWaiters/totalOperations; } }


        public RProcessor(List<SystemTask> AllTasks)
        {
            allTasks = new List<SystemTask>(AllTasks);
        }

        public void StartProcess()//Начать выполнение процесса
        {
            Random rnd = new Random();

            UpdateCurrentExecutionTasks();//Обновляем текущие задачи (удаляем ненужные + сортируем)

            while (currentExecutionTasks.Count != 0)//Пока есть текущие задачи
            {
                int currentIndex = rnd.Next(0, currentExecutionTasks.Count);

                for (int i = 0; i<currentExecutionTasks.Count; i++)//Перечисляем отсортированные текущие задачи
                {
                    if (i==currentIndex)
                        currentExecutionTasks[i].Execute();//Выполняем первую текущую задачу
                    else
                    {
                        currentExecutionTasks[i].Wait();//Остальные задачи ожидают
                        totalWaiters++;
                    }

                    totalOperations++;
                }

                UpdateCurrentExecutionTasks();//Обновляем текущие задачи (удаляем ненужные + сортируем)
            }
        }

        private void UpdateCurrentExecutionTasks()//Обновление и сортировка текущих задач
        {
            foreach (var item in allTasks)//Перечиляем все задачи
            {
                if (currentExecutionTasks.Contains(item))//Если эта задача есть в текущих, то пропускаем
                    continue;

                if (item.IsValid())//Если задача готова к началу работы, то добавляем в текущие
                    currentExecutionTasks.Add(item);
                else if (item.TimeDelay>0)//Если не готова, то уменьшаем ей задержку до выполнения
                    item.TimeDecrease();
            }

            List<SystemTask> toRemove = new List<SystemTask>();//Задачи к удалению из текущих

            foreach (var item in currentExecutionTasks)//Перечисляем текущие задачи
                if (!item.IsValid())//Если задача уже не акатуальна, то добавляем на удаление
                    toRemove.Add(item);

            foreach (var item in toRemove)//Перечисляем все задачи для удаления
                currentExecutionTasks.Remove(item);//Удаляем задачу
        }
    }
}