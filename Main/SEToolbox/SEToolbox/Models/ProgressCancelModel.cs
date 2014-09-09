﻿namespace SEToolbox.Models
{
    using System;
    using System.Diagnostics;
    using System.Timers;

    public class ProgressCancelModel : BaseModel
    {
        #region Fields

        private string _title;

        private string _subTitle;

        private string _dialogText;

        private double _progress;

        private double _maximumProgress;

        private TimeSpan? _estimatedTimeLeft;

        private Stopwatch _elapsedTimer;

        private readonly Stopwatch _progressTimer;

        private Timer _updateTimer;

        #endregion

        public ProgressCancelModel()
        {
            _progressTimer = new Stopwatch();
        }

        #region Properties

        public string Title
        {
            get { return _title; }

            set
            {
                if (value != _title)
                {
                    _title = value;
                    RaisePropertyChanged(() => Title);
                }
            }
        }

        public string SubTitle
        {
            get { return _subTitle; }

            set
            {
                if (value != _subTitle)
                {
                    _subTitle = value;
                    RaisePropertyChanged(() => SubTitle);
                }
            }
        }

        public string DialogText
        {
            get { return _dialogText; }

            set
            {
                if (value != _dialogText)
                {
                    _dialogText = value;
                    RaisePropertyChanged(() => DialogText);
                }
            }
        }

        public double Progress
        {
            get
            {
                return _progress;
            }

            set
            {
                if (value != _progress)
                {
                    _progress = value;

                    if (!_progressTimer.IsRunning || _progressTimer.ElapsedMilliseconds > 200)
                    {
                        RaisePropertyChanged(() => Progress);
                        System.Windows.Forms.Application.DoEvents();
                        _progressTimer.Restart();
                    }
                }
            }
        }

        public double MaximumProgress
        {
            get
            {
                return _maximumProgress;
            }

            set
            {
                if (value != _maximumProgress)
                {
                    _maximumProgress = value;
                    RaisePropertyChanged(() => MaximumProgress);
                }
            }
        }

        public TimeSpan? EstimatedTimeLeft
        {
            get
            {
                return _estimatedTimeLeft;
            }

            set
            {
                if (value != _estimatedTimeLeft)
                {
                    _estimatedTimeLeft = value;
                    RaisePropertyChanged(() => EstimatedTimeLeft);
                }
            }
        }

        #endregion

        #region methods

        public void ResetProgress(double initial, double maximumProgress)
        {
            MaximumProgress = maximumProgress;
            Progress = initial;
            _elapsedTimer = new Stopwatch();

            _updateTimer = new Timer(1000);
            var incrementTimer = 0;
            _updateTimer.Elapsed += delegate
            {
                var elapsed = _elapsedTimer.Elapsed;
                var estimate = new TimeSpan((long)(elapsed.Ticks / (Progress / _maximumProgress)));
                EstimatedTimeLeft = estimate - elapsed;

                if (incrementTimer == 10)
                {
                    _updateTimer.Interval = 5000;
                    incrementTimer++;
                }
                else
                    incrementTimer++;
            };

            _elapsedTimer.Restart();
            _updateTimer.Start();

            System.Windows.Forms.Application.DoEvents();
        }

        public void IncrementProgress()
        {
            Progress++;
        }

        public void ClearProgress()
        {
            if (_updateTimer != null)
            {
                _updateTimer.Stop();
                _updateTimer = null;
            }

            _elapsedTimer.Stop();
            Progress = 0;
        }

        #endregion
    }
}
