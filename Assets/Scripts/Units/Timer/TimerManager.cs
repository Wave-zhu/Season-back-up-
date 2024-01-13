using System;
using System.Collections.Generic;
using Units.Tools;
using UnityEngine;

namespace Units.Timer
{
    public class TimerManager : Singleton<TimerManager>
    {
        /* we need some Timer
         * create a queue to save idle Timer
         * create a list to save working Timer
         * update the Timer which is working
         * move the Timer to the idle Timer set once its work has been done
         */
        [SerializeField] private int _initMaxTimerCount;

        private Queue<GameTimer>_idleTimer =new Queue<GameTimer>();  
        private List<GameTimer> _workingTimer =new List<GameTimer>();

        private void InitTimerManager()
        {
            for (int i = 0; i < _initMaxTimerCount; i++)
            {
                CreateTimer();
            }
        }
        private void CreateTimer()
        {
            var timer = new GameTimer();
            _idleTimer.Enqueue(timer);
        }
        public void TryGetOneTimer(float time,Action task)
        {
            if(_idleTimer.Count == 0)
            {
                CreateTimer();
            }
            var timer = _idleTimer.Dequeue();
            timer.StartTimer(time, task);
            _workingTimer.Add(timer);
        }
        private void UpdateWorkingTimer()
        {
            if (_workingTimer.Count == 0) return;
            for(int i = 0; i < _workingTimer.Count; i++)
            {
                if (_workingTimer[i].GetTimerState() == TimerState.WORKING)
                {
                    _workingTimer[i].UpdateTimer();
                }
                else
                {
                    _idleTimer.Enqueue(_workingTimer[i]);
                    _workingTimer[i].ResetTimer();
                    _workingTimer.Remove(_workingTimer[i]);
                }
            }
        }
        private void Start()
        {
            InitTimerManager();
        }
        private void Update()
        {
            UpdateWorkingTimer();
        }


    }
}
