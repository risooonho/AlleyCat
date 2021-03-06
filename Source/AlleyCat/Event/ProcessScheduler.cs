using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using EnsureThat;
using Godot;
using Priority_Queue;

namespace AlleyCat.Event
{
    public class ProcessScheduler : IScheduler, IDisposable
    {
        public DateTimeOffset Now => DateTimeOffset.Now;

        private readonly IPriorityQueue<Task, double> _tasks = new SimplePriorityQueue<Task, double>();

        private readonly IDisposable _listener;

        public ProcessScheduler(IObservable<float> tick)
        {
            Ensure.That(tick, nameof(tick)).IsNotNull();

            _listener = tick.Subscribe(ProcessLoop);
        }

        public IDisposable Schedule<TState>(
            TState state, Func<IScheduler, TState, IDisposable> action) =>
            Schedule(state, Now.Ticks, action);

        public IDisposable Schedule<TState>(
            TState state,
            DateTimeOffset dueTime,
            Func<IScheduler, TState, IDisposable> action) =>
            Schedule(state, dueTime.Ticks, action);

        public IDisposable Schedule<TState>(
            TState state,
            TimeSpan dueTime,
            Func<IScheduler, TState, IDisposable> action) =>
            Schedule(state, Now.Ticks + dueTime.Ticks, action);

        private IDisposable Schedule<TState>(
            TState state,
            long dueTime,
            Func<IScheduler, TState, IDisposable> action)
        {
            Ensure.That(action, nameof(action)).IsNotNull();

            var task = new Task(dueTime, () => action(this, state));

            lock (_tasks)
            {
                _tasks.Enqueue(task, task.Ticks);
            }

            return task;
        }

        protected void ProcessLoop(float delta)
        {
            lock (_tasks)
            {
                var now = Now.Ticks;

                while (_tasks.Count > 0 && _tasks.First.Ticks <= now)
                {
                    try
                    {
                        _tasks.Dequeue().Execute();
                    }
                    catch (Exception e)
                    {
                        // TODO Use a logger to log errors.
                        GD.Print("ERROR - Failed to execute a task.");
                        GD.Print(e.ToString());
                    }
                }
            }
        }

        public void Dispose()
        {
            lock (_tasks)
            {
                _tasks.Clear();
            }

            _listener.Dispose();
        }

        private class Task : IDisposable
        {
            public long Ticks { get; }

            private readonly System.Action _action;

            private bool _done;

            public Task(long ticks, System.Action action)
            {
                Debug.Assert(action != null, "action != null");

                Ticks = ticks;

                _action = action;
                _done = false;
            }

            public void Execute()
            {
                if (!_done) _action.Invoke();
            }

            public void Dispose()
            {
                _done = true;
            }
        }
    }
}
