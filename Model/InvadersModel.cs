using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Invaders.Model
{
    class InvadersModel
    {
        public readonly static Size PlayAreaSize = new Size(400, 300);

        public const int MaximumPlayerShots = 3;
        public const int InitialStarCount = 50;

        private readonly Random _random = new Random();

        public int Score { get; private set; }
        public int Wave { get; private set; }
        public int Lives { get; private set; }

        public bool GameOver { get; private set; }

        private DateTime? _playerDied = null;

        public bool PlayerDying { get { return _playerDied.HasValue; } }

        private Player _player;

        private readonly List<Invader> _invaders = new List<Invader>();
        private readonly List<Shot> _playerShots = new List<Shot>();
        private readonly List<Shot> _invaderShots = new List<Shot>();
        private readonly List<Point> _stars = new List<Point>();

        private readonly Dictionary<InvaderType, int> _invaderScores = new Dictionary<InvaderType, int>()
        {
            {InvaderType.Bug, 40},
            {InvaderType.Satellite, 20},
            {InvaderType.Saucer, 30},
            {InvaderType.Spaceship, 50},
            {InvaderType.Star, 10},
            
        };

        
        private Direction _invaderDirection = Direction.Left;

        private bool _justMovedDown = false;

        private DateTime _lastUpdated = DateTime.MinValue;
        private double _framesSkipped;

        //private int i;

        public InvadersModel()
        {
            EndGame();
        }

        public void EndGame()
        {
            GameOver = true;
        }
        

        public void StartGame()
        {
            GameOver = false;

            foreach (Invader invader in _invaders)
            {
                OnShipChanged(invader, false);
            }
            _invaders.Clear();


            foreach (Shot shot in _playerShots)
            {
                OnShotMoved(shot, true);
            }
            _playerShots.Clear();

            foreach (Shot shot in _invaderShots)
            {
                OnShotMoved(shot, true);
            }
            _invaderShots.Clear();

            foreach (Point star in _stars)
            {
                OnStarChanged(star, true);
            }
            _stars.Clear();

            for (int i = 0; i < InitialStarCount; i++)
            {
                AddAStar();
            }

            _player = new Player();
            OnShipChanged(_player, false);

            Lives = 2;
            Wave = 0;

            NextWave();
        }        

        public void FireShot()
        {
            if (GameOver)
            {
                return;
            }
            if (_playerShots.Count < MaximumPlayerShots)
            {
                Shot shot = new Shot(new Point(_player.Location.X + _player.Area.Width / 2, _player.Location.Y), Direction.Up);
                _playerShots.Add(shot);
                OnShotMoved(shot, false);
            }
        }

        public void MovePlayer(Direction direction)
        {
            if (PlayerDying) return;

            _player.Move(direction);
            OnShipChanged(_player, false);

        }

        public void Twinkle()
        {
            int coinFlip = _random.Next(2);

            if (coinFlip == 0 && _stars.Count < InitialStarCount*1.5)
            {
                AddAStar();
            }
            else if(_stars.Count > InitialStarCount*0.85)
            {
                RemoveAStar();
            }
        }        

        public void Update(bool paused)
        {
            Twinkle();
           
            

            if (!paused)
            {
                if (_invaders.Count() == 0) NextWave();
                if (!PlayerDying)
                {
                    MoveInvaders();
                    MoveShots();
                    ReturnFire();
                    CheckForInvaderCollisions();
                    CheckForPlayerCollisions();
                    CheckForBottomCollisions();
                }
                else if (_playerDied.HasValue && (DateTime.Now - _playerDied > TimeSpan.FromSeconds(2.5)))
                {
                    _playerDied = null;
                    OnShipChanged(_player, false);
                }
            }
        }
        

        private void AddAStar()
        {
            int starLocationX = _random.Next(10, (int)PlayAreaSize.Width - 10);
            int starLocationY = _random.Next(10, (int)PlayAreaSize.Height - 10);

            Point star = new Point(starLocationX,starLocationY);

            if (!_stars.Contains(star))
            {
                _stars.Add(star);
                OnStarChanged(star, false);
            }
        }

        ///new method
        //private void ShootingStar()
        //{

        //    Point star = _stars[_random.Next(_stars.Count)];
        //    if (_stars.Contains(star))
        //    {
        //        int starLocationX = _random.Next(10, (int)PlayAreaSize.Width - 10);
        //        int starLocationY = _random.Next(10, (int)PlayAreaSize.Height - 10);

        //        star.X = starLocationX;
        //        star.Y = starLocationY;
        //        OnStarChanged(star, false);
        //    }
         
        //}

        private void MoveInvaders()
        {
            TimeSpan timeSinceLastMovement = DateTime.Now - _lastUpdated;
            _framesSkipped = Math.Max(7 - Wave, 1) * (2 * _invaders.Count);

            if (timeSinceLastMovement >= TimeSpan.FromMilliseconds(_framesSkipped))
            {
                _lastUpdated = DateTime.Now;

                if (_invaderDirection == Direction.Right)
                {
                    var rightBoundryReachers =
                        from invader in _invaders
                        where ((invader.Area.Right + Invader.HorizontalPixelsPerMove) >= PlayAreaSize.Width)
                        select invader;

                    if (rightBoundryReachers.Count() > 0)
                    {
                        _invaderDirection = Direction.Down;

                        foreach (Invader invader in _invaders)
                        {
                            invader.Move(_invaderDirection);
                            OnShipChanged(invader, false);
                        }
                        _justMovedDown = true;
                        _invaderDirection = Direction.Left;
                    }
                    else
                    {
                        _justMovedDown = false;
                        foreach (Invader invader in _invaders)
                        {
                            invader.Move(_invaderDirection);
                            OnShipChanged(invader, false);
                        }
                    }
                }
                else if (_invaderDirection == Direction.Left)
                {
                    var leftBoundryReachers =
                        from invader in _invaders
                        where (invader.Area.Left < Invader.HorizontalPixelsPerMove)
                        select invader;

                    if (leftBoundryReachers.Count() > 0)
                    {
                        _invaderDirection = Direction.Down;

                        foreach (Invader invader in _invaders)
                        {
                            invader.Move(_invaderDirection);
                            OnShipChanged(invader, false);
                        }
                        _justMovedDown = true;
                        _invaderDirection = Direction.Right;
                    }
                    else
                    {
                        _justMovedDown = false;
                        foreach (Invader invader in _invaders)
                        {
                            invader.Move(_invaderDirection);
                            OnShipChanged(invader, false);
                        }
                    }
                }
            }            
        }

        private void MoveShots()
        {
            //handle player shots

            foreach (Shot shot in _playerShots)
            {
                shot.Move();
                OnShotMoved(shot, false);
            }

            var shotsOffTheEdge =
                from shot in _playerShots
                where (shot.Location.Y < 10 || shot.Location.Y > PlayAreaSize.Height -10)
                select shot;

            foreach (Shot shot in shotsOffTheEdge.ToList())
            {
                _playerShots.Remove(shot);
                OnShotMoved(shot, true);
            }

            //handle invaders shots

            foreach (Shot shot in _invaderShots)
            {
                shot.Move();
                OnShotMoved(shot, false);
            }

            var invadersShotsOffTheEdge =
                from shot in _invaderShots
                where (shot.Location.Y > PlayAreaSize.Height - 10)
                select shot;

            foreach (Shot shot in invadersShotsOffTheEdge.ToList())
            {
                
                OnShotMoved(shot, true);
                _invaderShots.Remove(shot);
            }
        }

        private void RemoveAStar()
        {
            if (_stars.Count <= 0) return;
            int randomStar = _random.Next(_stars.Count);
            OnStarChanged(_stars[randomStar], true);
            _stars.Remove(_stars[randomStar]);

        }

        private void NextWave()
        {
            Wave++;
            _invaders.Clear();

            InvaderType invaderType;

            for (int row = 0; row < 6; row++)
            {
                switch (row)
                {
                    case 0:
                        invaderType = InvaderType.Spaceship;
                        break;
                    case 1:
                        invaderType = InvaderType.Bug;
                        break;
                    case 2:
                        invaderType = InvaderType.Saucer;
                        break;
                    case 3:
                        invaderType = InvaderType.Satellite;
                        break;
                    default:
                        invaderType = InvaderType.Star;
                        break;
                }

                for (int column = 0; column < 11; column++)
                {
                    Point location = new Point(column * Invader.InvaderSize.Width * 1.4, row * Invader.InvaderSize.Height * 1.4);
                    Invader newInvader = new Invader(invaderType, location, _invaderScores[invaderType]);
                    _invaders.Add(newInvader);
                    OnShipChanged(newInvader, false);

                    //_invaders.Add(new Invader(invaderType, location, _invaderScores[invaderType]));
                    //OnShipChanged(_invaders[_invaders.Count - 1], false);
                }
            }
            
        }

        private void ReturnFire()
        {
            // EVALUATE later!! 

            if (_invaders.Count() == 0) return;
            if (_invaderShots.Count >= (Wave + 1)) return;
            if (_random.Next(10) < 10 - Wave) return;

            var invadersGroups =
                from invader in _invaders
                orderby invader.Location.X descending
                group invader by invader.Location.X
                into invaderGroups                
                select invaderGroups;

            var randomGroup = invadersGroups.ElementAt(_random.Next(invadersGroups.Count()));
            Invader shooter = randomGroup.Last();

            Shot newShot = new Shot(new Point(shooter.Area.X + shooter.Area.Width / 2 - 1, shooter.Area.Bottom), Direction.Down);
            _invaderShots.Add(newShot);
            OnShotMoved(newShot, false);         
        }

        private void CheckForInvaderCollisions()
        {

            //List<Shot> shotsHit = new List<Shot>();
            //List<Invader> invadersKilled = new List<Invader>();
            //foreach (Shot shot in _playerShots)
            //{
            //    var invadersShot = from invader in _invaders
            //                 where invader.Area.Contains(shot.Location) == true && shot.Direction == Direction.Up
            //                 select new { InvaderKilled = invader, ShotHit = shot };

            //    if (invadersShot.Count() > 0)
            //    {
            //        foreach (var invader in invadersShot)
            //        {
            //            shotsHit.Add(invader.ShotHit);
            //            invadersKilled.Add(invader.InvaderKilled);
            //        }
            //    }
            //}
            //foreach (Invader invader in invadersKilled)
            //{
            //    Score += invader.Score;
            //    _invaders.Remove(invader);
            //    OnShipChanged(invader, true);
            //}
            //foreach (Shot shot in shotsHit)
            //{
            //    _playerShots.Remove(shot);
            //    OnShotMoved(shot, true);
            //}

            List<Shot> playerShots = _playerShots.ToList();
            List<Invader> invaders = _invaders.ToList();

            foreach (Shot shot in playerShots)
            {
                Rect shotRect = new Rect(shot.Location.X, shot.Location.Y, Shot.ShotSize.Width, Shot.ShotSize.Height);

                var invadersShot =
                    from invader in invaders
                    where RectOverlap(invader.Area, shotRect)
                    select invader;

                foreach (Invader shotInvader in invadersShot.ToList())
                {
                    Score += shotInvader.Score;
                    _invaders.Remove(shotInvader);
                    OnShipChanged(shotInvader, true);
                    _playerShots.Remove(shot);
                    OnShotMoved(shot, true);
                }

            }
        }

        private void CheckForPlayerCollisions()
        {
            List<Shot> invaderShots = _invaderShots.ToList();

            foreach (Shot shot in invaderShots)
            {
                Rect shotRect = new Rect(shot.Location.X, shot.Location.Y, Shot.ShotSize.Width, Shot.ShotSize.Height);

                if (RectOverlap(shotRect, _player.Area))
                {
                    Lives--;
                    if (Lives >= 0)
                    {                        
                        _playerDied = DateTime.Now;
                        OnShipChanged(_player, true);
                        _invaderShots.Remove(shot);
                        OnShotMoved(shot, true);
                    }
                    else
                    {                       
                        //remove all shots?
                        
                        EndGame();
                    }
                }
                    
            }
        }

        private void CheckForBottomCollisions()
        {
            List<Invader> invaders = _invaders.ToList();

            var bottomReachers =
                from invader in invaders
                where (invader.Area.Bottom + Invader.VerticalPixelsPerMove >= PlayAreaSize.Height - 30)
                select invader;

            if (_justMovedDown && bottomReachers.Count() > 0)
            {
                EndGame();
            }
        }

        private static bool RectOverlap(Rect r1, Rect r2)
        {
            r1.Intersect(r2);
            if (r1.Width > 0 || r1.Height > 0)
                return true;            
            return false;
        }

        public void UpdateAllShipsAndStars()
        {
            foreach (Shot shot in _playerShots)
                OnShotMoved(shot, false);
            foreach (Shot shot in _invaderShots)
                OnShotMoved(shot, false);
            foreach (Ship ship in _invaders)
                OnShipChanged(ship, false);
            foreach (Point star in _stars)
                OnStarChanged(star, false);
            OnShipChanged(_player, false);
        }


        // EVENT HANDLERS

        public event EventHandler<ShotMovedEventArgs> ShotMoved;

        public void OnShotMoved(Shot shot, bool disapeared)
        {
            EventHandler<ShotMovedEventArgs> shotMoved = ShotMoved;
            if (shotMoved != null)
            {
                shotMoved(this, new ShotMovedEventArgs(shot, disapeared));
            }
        }

        public event EventHandler<StarChangedEventArgs> StarChanged;

        public void OnStarChanged(Point star, bool disappeared)
        {
            EventHandler<StarChangedEventArgs> starChanged = StarChanged;
            if (starChanged != null)
            {
                starChanged(this, new StarChangedEventArgs(star, disappeared));
            }
        }

        public event EventHandler<ShipChangedEventArgs> ShipChanged;

        public void OnShipChanged(Ship shipThatChanged, bool killed)
        {
            EventHandler<ShipChangedEventArgs> shipChanged = ShipChanged;
            if (shipChanged != null)
            {
                shipChanged(this, new ShipChangedEventArgs(shipThatChanged, killed));
            }
        }
    }
}
