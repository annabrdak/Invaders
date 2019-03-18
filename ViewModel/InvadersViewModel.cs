using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;



namespace Invaders.ViewModel
{
    using View;
    using Model;
    using System.ComponentModel;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using DispatcherTimer = System.Windows.Threading.DispatcherTimer;
    using FrameworkElement = System.Windows.FrameworkElement;

    class InvadersViewModel : INotifyPropertyChanged
    {
        

        private readonly ObservableCollection<FrameworkElement> _sprites = new ObservableCollection<FrameworkElement>();
        public INotifyCollectionChanged Sprites { get { return _sprites; } }

        public bool GameOver { get { return _model.GameOver; } }

        private readonly ObservableCollection<object> _lives = new ObservableCollection<object>();
        public INotifyCollectionChanged Lives { get { return _lives; } }

        public bool Paused { get; set; }
        private bool _lastPaused = true;

        public static double Scale { get; private set; }

        public int Score { get; private set; }

        public Size PlayAreaSize
        {
            set
            {
                Scale = value.Width / 405;
                _model.UpdateAllShipsAndStars();
                RecreateScanLines();
            }
        }

        private void EndGame()
        {
            _model.EndGame();
        }

        private readonly InvadersModel _model = new InvadersModel();
        private readonly DispatcherTimer _timer = new DispatcherTimer();

        private FrameworkElement _playerControl = null;

        private bool _playerFlashing = false;

        private readonly Dictionary<Invader, FrameworkElement> _invaders =
            new Dictionary<Invader, FrameworkElement>();

        private readonly Dictionary<FrameworkElement, DateTime> _shotInvaders =
            new Dictionary<FrameworkElement, DateTime>();

        private readonly Dictionary<Shot, FrameworkElement> _shots =
            new Dictionary<Shot, FrameworkElement>();

        private readonly Dictionary<Point, FrameworkElement> _stars =
            new Dictionary<Point, FrameworkElement>();

        private readonly List<FrameworkElement> _scanLines =
            new List<FrameworkElement>();


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public InvadersViewModel()
        {
            Scale = 1;

            _model.ShipChanged += ModelShipChangedEventHandler;
            _model.ShotMoved += ModelShotMovedEventHandler;
            _model.StarChanged += ModelStarChangedEventHandler;

            _timer.Interval = TimeSpan.FromMilliseconds(66);
            _timer.Tick += TimerTickEventHandler;
            EndGame();
        }

        private void TimerTickEventHandler(object sender, EventArgs e)
        {
            if (_lastPaused != Paused)
            {
                OnPropertyChanged("Paused");
                _lastPaused = Paused;
            }
            if (!Paused)
            {
                if (_leftAction.HasValue && _rightAction.HasValue)
                {
                    _model.MovePlayer(_leftAction > _rightAction ? Direction.Left : Direction.Right);
                }
                else if (_leftAction.HasValue)
                {
                    _model.MovePlayer(Direction.Left);
                }
                else if (_rightAction.HasValue)
                {
                    _model.MovePlayer(Direction.Right);
                }
            }
            _model.Update(Paused);

            if (Score != _model.Score)
            {
                Score = _model.Score;
                OnPropertyChanged("Score");
            }
            if (_model.Lives >= 0)
            {
                if (_lives.Count != _model.Lives)
                {
                    while (_lives.Count < _model.Lives)
                    {
                        _lives.Add(new object());
                    }
                    while (_lives.Count > _model.Lives)
                    {
                        _lives.RemoveAt(0);                       
                    }                    
                }
            }
            foreach (FrameworkElement control in _shotInvaders.Keys.ToList())
            {
                if (DateTime.Now - _shotInvaders[control] > TimeSpan.FromMilliseconds(500))
                {
                    _sprites.Remove(control);
                    _shotInvaders.Remove(control);
                }
            }
            if (_model.GameOver)
            {
                OnPropertyChanged("GameOver");
                _timer.Stop();
            }

        }

        void ModelStarChangedEventHandler(object sender, StarChangedEventArgs e)
        {
            if (e.Disappeared && _stars.ContainsKey(e.Point))
            {
                FrameworkElement control = _stars[e.Point];
                _sprites.Remove(control);
            }
            else
            {
                if (!_stars.ContainsKey(e.Point))
                {
                    FrameworkElement control = InvadersHelper.StarControlFactory(e.Point, Scale);
                    _stars.Add(e.Point,control);
                    _sprites.Add(control);
                }
                else
                {
                    //Stars typically won't change locations, so this else clause probably won't
                    //get hit—but you can use it to add shooting stars if you
                    //want.Look up the star's control in _stars and use a
                    //helper method to move it to the new location.
                    //Random random = new Random();
                    
                }
            }
        }

        private void ModelShotMovedEventHandler(object sender, ShotMovedEventArgs e)
        {
            if (!e.Disappeared)
            {
                if (!_shots.ContainsKey(e.Shot))
                {
                    FrameworkElement control = InvadersHelper.ShotControlFactory(e.Shot, Scale);
                    _shots.Add(e.Shot, control);
                    _sprites.Add(control);
                }
                else
                {
                    FrameworkElement control = _shots[e.Shot];
                    InvadersHelper.MoveElementOnCanvas(control, e.Shot.Location.X * Scale, e.Shot.Location.Y* Scale);
                }
            }
            else
            {
                if (_shots.ContainsKey(e.Shot))
                {                    
                    _sprites.Remove(_shots[e.Shot]);
                    _shots.Remove(e.Shot);
                }
            }
        }

        private void ModelShipChangedEventHandler(object sender, ShipChangedEventArgs e)
        {
            if (!e.Killed)
            {
                if (e.ShipUpdated is Invader)
                {
                    Invader invader = e.ShipUpdated as Invader;
                    if (!_invaders.Keys.Contains(invader))
                    {
                        FrameworkElement control = InvadersHelper.ShipControlFactory(invader, Scale);
                        _invaders.Add(invader, control);
                        _sprites.Add(control);
                    }
                    else
                    {
                        FrameworkElement control = _invaders[invader];
                        InvadersHelper.MoveElementOnCanvas(control,invader.Location.X*Scale,invader.Location.Y* Scale);
                        InvadersHelper.ResizeElement(control, invader.Size.Width * Scale, invader.Size.Height * Scale);
                    }
                }
                else if (e.ShipUpdated is Player)
                {
                    if (_playerFlashing)
                    {
                        _playerFlashing = false;
                        AnimatedImage playerImage = _playerControl as AnimatedImage;
                        if (playerImage != null)
                            playerImage.StopFlashing();
                    }

                    Player player = e.ShipUpdated as Player;
                    if (_playerControl == null)
                    {
                        _playerControl = InvadersHelper.ShipControlFactory(player, Scale);
                        _sprites.Add(_playerControl);
                    }
                    else
                    {
                        InvadersHelper.MoveElementOnCanvas(_playerControl, player.Location.X * Scale, player.Location.Y * Scale);
                        InvadersHelper.ResizeElement(_playerControl, player.Size.Width * Scale, player.Size.Height * Scale);
                    }
                }
            }
            else
            {
                if (e.ShipUpdated is Invader)
                {
                    Invader invader = e.ShipUpdated as Invader;

                    if (!_invaders.ContainsKey(invader)) return;

                    FrameworkElement control = _invaders[invader];
                    AnimatedImage invaderImage = control as AnimatedImage;

                    if (invader != null)
                    {
                        invaderImage.InvaderShot();
                        _shotInvaders.Add(control, DateTime.Now);
                        _invaders.Remove(invader);
                    }
                }
                else if (e.ShipUpdated is Player)
                {
                    AnimatedImage playerImage = _playerControl as AnimatedImage;
                    playerImage.StartFlashing();
                    _playerFlashing = true;
                }
            }
        }

        public void StartGame()
        {
            Paused = false;
            foreach (var invader in _invaders.Values) _sprites.Remove(invader);
            foreach (var shot in _shots.Values) _sprites.Remove(shot);
            _model.StartGame();
            OnPropertyChanged("GameOver");
            _timer.Start();
        }

        public void PauseResumeGame()
        {
            if (GameOver) return;
            if (!Paused)
            {
                Paused = true;                
                _timer.Stop();
            }
            else if (Paused)
            {
                Paused = false;
                _timer.Start();
            }
            OnPropertyChanged("Paused");
        }

        private void RecreateScanLines()
        {
            foreach (FrameworkElement scanLine in _scanLines)
                if (_sprites.Contains(scanLine))
                    _sprites.Remove(scanLine);
            _scanLines.Clear();
            for (int y = 0; y < 300; y += 2)
            {
                FrameworkElement scanLine = InvadersHelper.ScanLineFactory(y, 400, Scale);
                _scanLines.Add(scanLine);
                _sprites.Add(scanLine);
            }
        }

        private DateTime? _leftAction = null;
        private DateTime? _rightAction = null;

        internal void KeyUp(Key key)
        {
            if (key == Key.Left)
            {
                _leftAction = null;
            }
            if (key == Key.Right)
            {
                _rightAction = null;
            }

        }

        internal void KeyDown(Key key)
        {
            if (key == Key.Space)
            {
                _model.FireShot();
            }
            if (key == Key.Left)
            {
                
                _leftAction = DateTime.Now;
            }
            if (key == Key.Right)
            {
               
                _rightAction = DateTime.Now;
            }
        }
    }
}
