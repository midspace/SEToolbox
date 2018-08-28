namespace SEToolbox.Models
{
    using System;
    using System.Diagnostics;
    using System.Timers;

    public class ProgressCancelModel : BaseModel, IDisposable
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

        ~ProgressCancelModel()
        {
            Dispose(false);
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
                    OnPropertyChanged(nameof(Title));
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
                    OnPropertyChanged(nameof(SubTitle));
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
                    OnPropertyChanged(nameof(DialogText));
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
                        OnPropertyChanged(nameof(Progress));
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
                    OnPropertyChanged(nameof(MaximumProgress));
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
                    OnPropertyChanged(nameof(EstimatedTimeLeft));
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
                TimeSpan elapsed = _elapsedTimer.Elapsed;
                TimeSpan estimate = elapsed;
                if (Progress > 0)
                    estimate = new TimeSpan((long)(elapsed.Ticks / (Progress / _maximumProgress)));

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_updateTimer != null)
                {
                    _updateTimer.Stop();
                    _updateTimer.Dispose();
                }
                if (_progressTimer != null)
                {
                    _progressTimer.Stop();
                }
            }
        }

        #endregion
    }
}
