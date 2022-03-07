using System;
using System.Linq;

namespace Лаба1
{
    public class Program
    {
        static Dictionary<int, int> consolePositions = new Dictionary<int, int>();//Словрь координат x по строчкам y   y - ключ   x - значение

        static void Main(string[] args)
        {
            List<SystemTask> tasks = new List<SystemTask>() { 
            new SystemTask(6,()=>SetPrint(0,"И"),()=>SetPrint(0,"Г"),0,()=>SetPrint(0," "),0),
            new SystemTask(2,()=>SetPrint(1,"И"),()=>SetPrint(1,"Г"),2,()=>SetPrint(1," "),0),
            new SystemTask(7,()=>SetPrint(2,"И"),()=>SetPrint(2,"Г"),6,()=>SetPrint(2," "),0),
            new SystemTask(5,()=>SetPrint(3,"И"),()=>SetPrint(3,"Г"),0,()=>SetPrint(3," "),0),
            };

            SJFProcessor processor = new SJFProcessor(tasks);

            processor.StartProcess();

            Console.ReadKey();
        }

        private static void SetPrint(int y,string text)
        {
            if (!consolePositions.ContainsKey(y))
                consolePositions.Add(y, 0);

            int xPos = consolePositions[y];

            Console.SetCursorPosition(xPos,y);
            Console.Write(text);

            consolePositions[y] += text.Length + 1;
        }
    }

    public class SystemTask
    {
        private Action onExecute;//Вызывается при выполнении задачи
        private Action onWait;//Вызывается при ожидании задачи
        private Action onTimeDelayed;//Вызывается при задержке задачи (отложенный запуск)

        private int startTimeDelay = 0;//Начальное время задержки
        private int timeDelay = 0;//Оставщееся время задержки
        private int priority = 0;//Приоритет
        private int executeTimes = 0;//Количество выполнений

        public int TimeDelay { get { return timeDelay; } }
        public int StartTimeDelay { get { return startTimeDelay; } }
        public int Priority { get { return priority; } }
        public int ExecuteTimes { get { return executeTimes; } }

        public SystemTask(int ExecuteTimes, Action OnExecute, Action OnWait, int TimeDelay = 0,Action OnTimeDelayed = null, int Priority = 0)
        {
            onExecute = OnExecute;
            onWait = OnWait;
            timeDelay = TimeDelay;
            onTimeDelayed = OnTimeDelayed;
            startTimeDelay = TimeDelay;
            priority = Priority;
            executeTimes = ExecuteTimes;
        }

        public bool IsValid()//Актуальна ли задача
        {
            return timeDelay==0 && executeTimes>0;
        }

        public void Wait()//Вызывается при выполнении других задач
        {
            if (!IsValid())
                return;

            onWait?.Invoke();
        }

        public void Execute()//Вызывается при выполнении этой задачи
        {
            if (!IsValid())
                return;

            onExecute?.Invoke();
            executeTimes--;
        }

        public void TimeDecrease()//Вызывается при ожидании задачи
        {
            timeDelay -=1;

            if (timeDelay<0)
                timeDelay = 0;
            else
                onTimeDelayed?.Invoke();
        }
    }

    public class SJFProcessor
    {
        private List<SystemTask> allTasks = new List<SystemTask>();//Все задачи

        private List<SystemTask> currentExecutionTasks = new List<SystemTask>();//Текущие задачи

        public SJFProcessor(List<SystemTask> AllTasks)
        {
            allTasks = new List<SystemTask>(AllTasks);
        }

        public void StartProcess()//Начать выполнение процесса
        {
            UpdateCurrentExecutionTasks();//Обновляем текущие задачи (удаляем ненужные + сортируем)

            while (currentExecutionTasks.Count != 0)//Пока есть текущие задачи
            {
                for(int i=0;i<currentExecutionTasks.Count;i++)//Перечисляем отсортированные текущие задачи
                {
                    if (i==0)
                        currentExecutionTasks[i].Execute();//Выполняем первую текущую задачу
                    else
                        currentExecutionTasks[i].Wait();//Остальные задачи ожидают
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
                else if(item.TimeDelay>0)//Если не готова, то уменьшаем ей задержку до выполнения
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
}