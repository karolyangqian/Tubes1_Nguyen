using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// woff 🐶
// ------------------------------------------------------------------
// Targeting: Play It Forward
// Movement: Anti-Gravity & Stop and Go
// ------------------------------------------------------------------
/*

🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕

⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣤⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣾⣿⣿⣿⣦⣴⣶⣶⣦⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢻⣿⣿⣿⣿⣿⣿⣿⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣀⣤⣤⣤⣤⣤⣤⣤⣤⣀⣀⣀⠀⠀⠀⠀⢻⣿⣿⣿⣿⣿⡿⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣤⣶⠿⠟⠛⠛⠋⠉⠉⠉⠉⠉⠉⠛⠛⠛⠿⢷⣦⣤⣀⡹⠿⠿⠛⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⣠⣤⣴⣶⣶⣾⠟⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠙⠻⣿⣿⣶⣶⣶⣤⣄⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⣴⣿⠟⠉⠀⠀⠙⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠟⠀⠀⠀⠉⠙⢿⣦⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⣠⣿⡟⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢦⣽⣿⡄⠀⠀⠀⠀⠀
⠀⠀⠀⣰⣿⠏⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠙⣿⣷⠀⠀⠀⠀⠀
⠀⠀⢰⣿⡏⣤⠀⠀⠀⠀⠀⢀⡼⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢰⣻⡀⠀⠀⢤⢠⣼⣿⡆⠀⠀⠀⠀
⠀⠀⠀⢿⣿⠁⠀⠀⠀⠀⣴⡾⠁⠀⠀⠀⢀⣀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣠⣀⠀⠀⠀⠀⠀⠈⢻⣇⠀⠀⠈⣇⣿⣿⠀⠀⠀⠀⠀
⠀⠀⠀⢸⣿⠀⡀⣀⠀⢠⣿⠃⠀⠀⢀⣾⣿⣿⡿⠆⠀⠀⠀⠀⠀⠀⠀⠀⠀⣼⣿⣿⣿⡷⠀⠀⠀⠀⠀⢸⣿⠀⢠⣠⣿⣿⠇⠀⠀⠀⠀⠀
⠀⠀⠀⠈⢿⣷⣇⣽⠀⢈⡏⠀⠀⠀⠸⣿⣿⣿⣦⣤⠀⠀⠀⠀⠀⠀⠀⠀⠀⢻⣿⣿⣧⣤⠥⠀⠀⠀⠀⣿⣿⣧⣾⣿⠟⠁⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠈⠛⠿⣿⣧⣾⣿⡄⠀⠀⠀⠙⠿⠿⠿⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠛⠛⠛⠋⠀⠀⠀⠀⠀⢸⣿⡿⠋⠁⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠈⠉⣿⡇⣴⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣤⣤⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠐⢶⣼⣿⣀⣠⣤⣤⣤⣀⠀⠀⠀⠀⠀
⠀⠀⣠⣶⣾⠿⠛⠛⠻⢷⣿⣿⠁⠀⠀⠀⠀⠀⠀⠀⠀⣼⣿⣿⣿⣿⡆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⣿⣿⡿⠋⠉⠉⠉⠛⢿⣦⡀⠀⠀
⢀⣾⡿⠋⠀⠀⠀⠀⠀⠀⠙⣿⡆⢀⠀⠀⠀⠀⠀⠀⠀⠘⢿⣿⣿⠟⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⣤⣿⡟⠀⠀⠀⠀⠀⠀⠀⠹⣿⡆⠀
⣼⡿⠁⠀⠀⠀⠀⠀⠀⠀⠀⣸⣷⣿⣷⣧⠀⢀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣄⠀⢠⡾⣠⣇⣠⣿⣿⣿⡇⠀⢀⠀⠀⠀⢀⠀⠀⢹⣷⠀
⣿⣷⡀⠀⣷⠀⠀⠀⣼⣦⣴⣿⠏⠙⠻⠿⣷⡿⠷⣶⣶⡾⠿⠿⠷⢶⣶⣦⣤⣾⣿⣷⣿⣿⠿⠿⠛⠛⠙⠻⣿⣤⣾⣇⠀⢀⣸⣇⣀⣼⣿⠃
⠘⢿⣿⣾⣿⣷⣴⣾⡿⠟⠋⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠉⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠙⠛⠻⠿⠿⠿⠟⠛⠛⠁⠀

🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕

v1.1
- Fix Hitting Wall in do Stop and Go 
- Remove MIN_ENERGY
- Change Grav Calculation
- Add Stop and Go color
- Add Head-on fallback color
- Add and fix graphical debugging
- Add updated enemy data onScan
- Add knob for GRAV_OVERRIDE_TRESHOLD

🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕🐕

*/
// ------------------------------------------------------------------
public class Woff : Bot
{
    // Knobs
    private readonly static double  ENEMY_ENERGY_THRESHOLD = 3.5;
    private readonly static double  MOVE_WALL_MARGIN = 25;
    private readonly static double  GUN_FACTOR = 5;
    private readonly static double  RADAR_LOCK = 0.7;
    private readonly static double  MIN_RADIUS = 100;
    private readonly static double  DELTA_RADIUS = 100;
    private readonly static double  ITERATE_RADIUS = 3;
    private readonly static double  POINT_COUNT = 36;
    private readonly static double  MIN_DIVISOR = 1e-6;
    private readonly static double  GRAV_OVERRIDE_TRESHOLD = 0.9;
    private readonly static int     SAG_LIMIT = 3;
    private readonly static int     NGRAM_ORDER = 4;
    private readonly static int     BULLET_OFFSET_ARENA = 50;
    private readonly static int     ENEMY_GRAVITY_CONSTANT = 300;
    private readonly static int     BULLET_GRAVITY_CONSTANT = 10;
    private readonly static int     LAST_LOC_GRAVITY_CONSTANT = 10;
    private readonly static int     CORNER_CONSTANT = 100;

    // Global variables
    static int targetId;
    static double targetDistance;
    static double enemyDistance;
    static double pifDir;

    static double destX;
    static double destY;

    static int sag = 1;
    static int hitsag;
    static bool dontsag;

    Random rand = new Random();

    static Dictionary<int, EnemyData> enemyData = new Dictionary<int, EnemyData>();

    static List<Bullet> bullets;
    static List<MyBullet> myBullets;

    static void Main()
    {
        new Woff().Start();
    }

    Woff() : base(BotInfo.FromFile("woff.json")) { }

    public override void Run()
    {
        Console.WriteLine("Woff woff woff 🐶! |---| round: " + RoundNumber);
        RadarColor = Color.White;
        TracksColor = Color.White;
        GunColor = Color.White;

        SetTurnRadarRight(double.PositiveInfinity);
        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        AdjustRadarForBodyTurn = true;

        targetDistance = double.PositiveInfinity;
        enemyDistance = double.PositiveInfinity;
        bullets = new List<Bullet>();
        myBullets = new List<MyBullet>();
        dontsag = false;
        hitsag = 0;
        pifDir = 0;
    }

    public override void OnTick(TickEvent e)
    {
        // sag color
        if (EnemyCount == 1 && !dontsag)
        {
            if (DistanceRemaining == 0)
            {
                TurretColor = Color.Black;
                ScanColor = Color.Black;
                BodyColor = Color.Black;
                BulletColor = Color.Black;
                RadarColor = Color.White;
                TracksColor = Color.White;
                GunColor = Color.White;
            }
            else
            {
                TurretColor = Color.White;
                ScanColor = Color.White;
                BodyColor = Color.White;
                BulletColor = Color.White;
                RadarColor = Color.Black;
                TracksColor = Color.Black;
                GunColor = Color.Black;
            }
        }

        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            Bullet bullet = bullets[i];
            bullet.X += bullet.Speed * Math.Cos(bullet.Direction);
            bullet.Y += bullet.Speed * Math.Sin(bullet.Direction);
            Graphics.FillEllipse(Brushes.Black, (float)bullet.X, (float)bullet.Y, 
                        (float)(3 * bullet.Power), (float)(3 * bullet.Power));
            // Console.WriteLine("BulletId: " + i + " X: " + bullet.X + " Y: " + bullet.Y);

            if (bullet.X < 0 - BULLET_OFFSET_ARENA || bullet.X > ArenaWidth + BULLET_OFFSET_ARENA || 
                bullet.Y < 0 - BULLET_OFFSET_ARENA || bullet.Y > ArenaHeight + BULLET_OFFSET_ARENA)
            {
                bullets.RemoveAt(i);
            }
            else 
            {
                bullets[i] = bullet;
            }
        }

        for (int i = myBullets.Count - 1; i >= 0; i--)
        {
            Bullet bullet = myBullets[i].BulletData;
            bullet.X += bullet.Speed * Math.Cos(bullet.Direction);
            bullet.Y += bullet.Speed * Math.Sin(bullet.Direction);
            Graphics.FillEllipse(myBullets[i].Type == 0 ? Brushes.Black : Brushes.Red, 
                        (float)bullet.X, (float)bullet.Y, 
                        (float)(3 * bullet.Power), (float)(3 * bullet.Power));
            // Console.WriteLine("BulletId: " + i + " X: " + bullet.X + " Y: " + bullet.Y);

            EnemyData data = enemyData[myBullets[i].Target];
            if (distance(data.LastX, data.LastY, bullet.X, bullet.Y) < 18)
            {
                data.Type[myBullets[i].Type] += 5;
                myBullets.RemoveAt(i);
            }
            else if (bullet.X < 0 - BULLET_OFFSET_ARENA || bullet.X > ArenaWidth + BULLET_OFFSET_ARENA || 
                bullet.Y < 0 - BULLET_OFFSET_ARENA || bullet.Y > ArenaHeight + BULLET_OFFSET_ARENA)
            {
                data.Type[myBullets[i].Type]--;
                myBullets.RemoveAt(i);
            }
            else 
            {
                myBullets[i].BulletData = bullet;
            }
        }

        if (hitsag > SAG_LIMIT) dontsag = true;
        if (!dontsag && EnemyCount == 1 && targetDistance > 250) return;
        
        // Anti-Gravity
        double bestX = X;
        double bestY = Y;
        double minGrav = double.PositiveInfinity;

        for (int i = 0; i < POINT_COUNT; i++)
        {
            double theta = (2 * Math.PI / POINT_COUNT) * i;
            
            for (int u = 0; u < ITERATE_RADIUS; u++) {
                double r = Math.Sqrt(Math.Pow(u * DELTA_RADIUS, 2) + Math.Pow(MIN_RADIUS, 2));
                
                double x = X + r * Math.Cos(theta);
                double y = Y + r * Math.Sin(theta);

                if (x < MOVE_WALL_MARGIN || x > ArenaWidth - MOVE_WALL_MARGIN ||
                    y < MOVE_WALL_MARGIN || y > ArenaHeight - MOVE_WALL_MARGIN)
                {
                    continue;
                }

                double grav = CalcGrav(x, y);
                if (grav < minGrav)
                {
                    minGrav = grav;
                    bestX = x;
                    bestY = y;
                }
                // Console.WriteLine("minGrav: " + minGrav + " Grav: " + grav + " X: " + x + " Y: " + y);

                int gravColor = (int) Math.Min(255, Math.Max(0, grav * 255 / 1000));
                Graphics.FillEllipse(new SolidBrush(Color.FromArgb(
                            gravColor, 255 - gravColor, 0)), 
                            (float) x, (float) y, 10, 10);
            }
        }

        if (minGrav < CalcGrav(destX, destY) * GRAV_OVERRIDE_TRESHOLD)
        {
            destX = bestX;
            destY = bestY;
        }

        double turn = toRad(BearingTo(destX, destY));
        SetTurnLeft(toDeg(Math.Tan(turn)));
        SetForward(DistanceTo(destX, destY) * Math.Cos(turn));

        // Anti-Gravity color
        TurretColor = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
        ScanColor = Color.FromArgb(105, 105, rand.Next(256));
        BodyColor = ScanColor;
        BulletColor = ScanColor;
        RadarColor = Color.White;
        TracksColor = Color.White;
        GunColor = Color.White;
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        // Update enemy data
        if (!enemyData.ContainsKey(e.ScannedBotId))
        {
            enemyData[e.ScannedBotId] = new EnemyData();
        }
        EnemyData data = enemyData[e.ScannedBotId];
        data.LastX = e.X;
        data.LastY = e.Y;
        data.IsAlive = true;
        double currentSpeed = e.Speed;
        data.LastSpeed = currentSpeed;
        double currentDirection = toRad(e.Direction);
        double angularVelocity = data.HasPrevious ? 
                                (currentDirection - data.LastDirection + Math.PI) % (2 * Math.PI) - Math.PI : 0;
        data.LastDirection = currentDirection;

        // Lock closest target
        double scannedDistance = enemyDistance = DistanceTo(e.X, e.Y);
        if (scannedDistance < targetDistance)
        {
            targetId = e.ScannedBotId;
        }
        else if (e.ScannedBotId != targetId && GunHeat != 0)
        {
            return;
        }
        targetDistance = scannedDistance;

        // Radar 
        double radarAngle = double.PositiveInfinity * NormalizeRelativeAngle(RadarBearingTo(e.X, e.Y));
        if (!double.IsNaN(radarAngle) && (GunHeat < RADAR_LOCK || EnemyCount == 1))
        {
            SetTurnRadarLeft(radarAngle);
        }

        // Fire control
        double firePower = Energy / DistanceTo(e.X, e.Y) * GUN_FACTOR;
        if (GunTurnRemaining == 0)
        {
            SetFire(firePower);
        }

        double bulletSpeed = CalcBulletSpeed(firePower);

        // Input Virtual Bullets
        double energyDrop = data.LastEnergy - e.Energy;
        if (0.1 <= energyDrop && energyDrop <= 3)
        {
            AddVirtualBullet(e.X, e.Y, CalcBulletSpeed(energyDrop), energyDrop, (180 + DirectionTo(e.X, e.Y)));
            AddLinearVirtualBullet(e.X, e.Y, CalcBulletSpeed(energyDrop), energyDrop);
            if (!dontsag && EnemyCount == 1 && DistanceRemaining == 0)
            {
                double direction = toRad(DirectionTo(e.X, e.Y) + (90 - 15 * (targetDistance / 1000)) * sag);
                double distance = (3 + (int)(energyDrop * 1.999999)) * 8;
                destX = X + Math.Cos(direction) * distance;
                destY = Y + Math.Sin(direction) * distance;
                Graphics.DrawRectangle(new Pen(Brushes.Blue), (float)destX, (float)destY, 20, 20);
                
                if (destX < MOVE_WALL_MARGIN || destX > ArenaWidth - MOVE_WALL_MARGIN ||
                    destY < MOVE_WALL_MARGIN || destY > ArenaHeight - MOVE_WALL_MARGIN)
                {
                    sag = -sag;
                    hitsag = 0;
                }
                double turn = (BearingTo(e.X, e.Y) + (90 - 15 * (targetDistance / 1000)) * sag) * Math.PI / 180;
                SetTurnLeft(toDeg(Math.Tan(turn)));
                SetForward(distance * Math.Sign(Math.Cos(turn)));
            }
            // Console.WriteLine("Bullet Speed: " + CalcBulletSpeed(energyDrop) + " Power: " + energyDrop);
        }
        data.LastEnergy = e.Energy;

        // Input State
        double acceleration = data.HasPrevious ? currentSpeed - data.LastSpeed : 0;
        State currentState = new State(angularVelocity, currentSpeed, acceleration);
        data.StateHistory.Add(currentState);

        if (data.StateHistory.Count >= NGRAM_ORDER)
        {
            List<State> contextStates = data.StateHistory.GetRange(data.StateHistory.Count - (NGRAM_ORDER - 1), NGRAM_ORDER - 1);
            StateSequence contextKey = new StateSequence(contextStates);
            if (!data.NgramTree.ContainsKey(contextKey))
            {
                data.NgramTree[contextKey] = new TransitionSegmentTree();
            }
            data.NgramTree[contextKey].Add(currentState);
        }
        data.HasPrevious = true;

        // Head-on fallback
        if (data.Type.IndexOf(data.Type.Max()) != 0)
        {
            BulletColor = Color.Red;
            SetTurnGunLeft(GunBearingTo(e.X, e.Y));
            return;
        }

        // --- Play It Forward ---
        double predictedX = e.X;
        double predictedY = e.Y;
        double predictedDirection = currentDirection;
        double predictedSpeed = currentSpeed;
        double simAngularVelocity = angularVelocity;
        State simCurrentState = currentState;
        int time = 0;

        List<State> simContext = null;
        if (data.StateHistory.Count >= NGRAM_ORDER - 1)
        {
            simContext = new List<State>(data.StateHistory.GetRange(
                            data.StateHistory.Count - (NGRAM_ORDER - 1), NGRAM_ORDER - 1));
        }

        while (time * bulletSpeed < DistanceTo(predictedX, predictedY) && time < 100)
        {
            if (simContext != null)
            {
                StateSequence simContextKey = new StateSequence(simContext);
                if (data.NgramTree.ContainsKey(simContextKey))
                {
                    State nextState = data.NgramTree[simContextKey].GetMostFrequent();
                    simAngularVelocity = nextState.AngularVelocity / 1024.0;
                    predictedSpeed += nextState.Acceleration;
                    simContext.RemoveAt(0);
                    simContext.Add(nextState);
                }
            }
            predictedDirection += simAngularVelocity;
            predictedX += predictedSpeed * Math.Cos(predictedDirection);
            predictedY += predictedSpeed * Math.Sin(predictedDirection);
            time++;
        }

        // Bullet's Wall Avoidance
        predictedX = Math.Max(MOVE_WALL_MARGIN, Math.Min(ArenaWidth - MOVE_WALL_MARGIN, predictedX));
        predictedY = Math.Max(MOVE_WALL_MARGIN, Math.Min(ArenaHeight - MOVE_WALL_MARGIN, predictedY));

        Graphics.DrawRectangle(new Pen(Brushes.Red), (float)predictedX, (float)predictedY, 20, 20);
        double bearingFromGun = GunBearingTo(predictedX, predictedY);
        pifDir = toRad(bearingFromGun);
        SetTurnGunLeft(bearingFromGun);

        // Update enemy position
        foreach (var enemy in enemyData)
        {
            if (enemy.Key != targetId && enemy.Value.IsAlive)
            {
                EnemyData enemyData = enemy.Value;
                enemyData.LastX += enemyData.LastSpeed * Math.Cos(enemyData.LastDirection);
                enemyData.LastY += enemyData.LastSpeed * Math.Sin(enemyData.LastDirection);
            }
        }
    }

    public override void OnBulletFired(BulletFiredEvent e)
    {
        AddMyVirtualBullet(X, Y, e.Bullet.Speed, e.Bullet.Power, pifDir, targetId, 0);
        EnemyData data = enemyData[targetId];
        AddMyVirtualBullet(X, Y, e.Bullet.Speed, e.Bullet.Power, 
                        toRad(DirectionTo(data.LastX, data.LastY)), targetId, 1);
    }

    public override void OnHitByBullet(HitByBulletEvent e)
    {
        if (EnemyCount == 1)
        {
            hitsag++;
        }
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        enemyData[e.VictimId].IsAlive = false;
        
        if (e.VictimId == targetId)
        {
            targetDistance = double.PositiveInfinity;
        }
    }

    // --- Helper Functions ---
    private double CalcGrav(double candidateX, double candidateY)
    {
        double grav = 0;

        foreach (EnemyData enemy in enemyData.Values)
        {
            if (enemy.IsAlive)
            {
                grav += ENEMY_GRAVITY_CONSTANT * (enemy.LastEnergy - ENEMY_ENERGY_THRESHOLD) / 
                        (distanceSq(candidateX, candidateY, enemy.LastX, enemy.LastY) + MIN_DIVISOR);
            }
        }

        foreach (Bullet bullet in bullets)
        {
            Line2D bulletLine = new Line2D(
                bullet.X - Math.Cos(bullet.Direction) * 10000, 
                bullet.Y - Math.Sin(bullet.Direction) * 10000, 
                bullet.X + Math.Cos(bullet.Direction) * 10000, 
                bullet.Y + Math.Sin(bullet.Direction) * 10000
            );
            
            double d = bulletLine.DistanceToPoint(candidateX, candidateY);
            grav += BULLET_GRAVITY_CONSTANT * bullet.Power / (d * d + MIN_DIVISOR);

        }

        grav += LAST_LOC_GRAVITY_CONSTANT * rand.NextDouble() / 
                (Math.Pow(DistanceTo(candidateX, candidateY), 2) + MIN_DIVISOR);
        if (targetId != 0)
            grav += targetDistance - DistanceTo(enemyData[targetId].LastX, enemyData[targetId].LastY);
            
        grav += CORNER_CONSTANT / distanceSq(candidateX, candidateY, 0, 0);
        grav += CORNER_CONSTANT / distanceSq(candidateX, candidateY, 0, ArenaHeight);
        grav += CORNER_CONSTANT / distanceSq(candidateX, candidateY, ArenaWidth, 0);
        grav += CORNER_CONSTANT / distanceSq(candidateX, candidateY, ArenaWidth, ArenaHeight);

        return grav * 1000;
    }
    
    private void AddVirtualBullet(double x, double y, double speed, double power, double direction)
    {
        Bullet bullet = new Bullet
        {
            Speed = speed,
            Direction = direction,
            X = x + 2 * speed * Math.Cos(direction),
            Y = y + 2 * speed * Math.Sin(direction),
            Power = power
        };
        bullets.Add(bullet);
    }
        
    private void AddLinearVirtualBullet(double x, double y, double speed, double power)
    {
        // Linear-nya karol
        double vb = CalcBulletSpeed(power);
        double myDir = toRad(Direction);
        double vxt = Speed * Math.Cos(myDir);
        double vyt = Speed * Math.Sin(myDir);
        double xt = X;
        double yt = Y;
        double a = Math.Pow(vxt, 2) + Math.Pow(vyt, 2) - Math.Pow(vb, 2);
        double b = 2 * (vxt * (xt - x) + vyt * (yt - y));
        double c = Math.Pow(xt - x, 2) + Math.Pow(yt - y, 2);
        double d = Math.Pow(b, 2) - 4 * a * c;
        double t1 = (-b + Math.Sqrt(d)) / (2 * a);
        double t2 = (-b - Math.Sqrt(d)) / (2 * a);
        double t = Math.Max(t1, t2);
        double predictedX = xt + vxt * t;
        double predictedY = yt + vyt * t;
        double linearDirection = Math.Atan2(predictedY - y, predictedX - x);
        Bullet bulletLinear = new Bullet
        {
            Speed = speed,
            Direction = linearDirection,
            X = x + 2 * speed * Math.Cos(linearDirection),
            Y = y + 2 * speed * Math.Sin(linearDirection),
            Power = power * 2
        };
        bullets.Add(bulletLinear);
    }

    private void AddMyVirtualBullet(double x, double y, double speed, double power, double direction, int target, int type)
    {
        MyBullet myBullet = new MyBullet
        (
            x + 2 * speed * Math.Cos(direction),
            y + 2 * speed * Math.Sin(direction),
            speed,
            direction,
            power,
            target,
            type
        );
        myBullets.Add(myBullet);
    }
    
    public double distanceSq(double x1, double y1, double x2, double y2)
    {
        return Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2);
    }

    public double distance(double x1, double y1, double x2, double y2)
    {
        return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
    }

    public double toRad(double degree)
    {
        return degree * Math.PI / 180;
    }

    public double toDeg(double radian)
    {
        return radian * 180 / Math.PI;
    }
}

public struct State
{
    public int AngularVelocity; // quantized: radian * 1024
    public int Speed;           // -8 -- 8
    public int Acceleration;    // -1 -- 1

    public State(double angularVelocity, double speed, double acceleration)
    {
        AngularVelocity = (int)(angularVelocity * 1024);

        Speed = (int)Math.Round(speed);
        
        double threshold = 0.1; 
        if (acceleration < -threshold)
            Acceleration = -1;
        else if (acceleration > threshold)
            Acceleration = 1;
        else
            Acceleration = 0;
    }

    public override bool Equals(object obj)
    {
        if (obj is State state)
        {
            return state.AngularVelocity == AngularVelocity &&
                   state.Speed == Speed &&
                   state.Acceleration == Acceleration;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return AngularVelocity.GetHashCode() ^ Speed.GetHashCode() ^ Acceleration.GetHashCode();
    }
}

public class StateSequence
{
    public List<State> States { get; }
    public StateSequence(IEnumerable<State> states)
    {
        States = new List<State>(states);
    }
    public override bool Equals(object obj)
    {
        if (obj is StateSequence seq)
        {
            if (States.Count != seq.States.Count)
                return false;
            for (int i = 0; i < States.Count; i++)
            {
                if (!States[i].Equals(seq.States[i]))
                    return false;
            }
            return true;
        }
        return false;
    }
    public override int GetHashCode()
    {
        int hash = 17;
        foreach (var s in States)
            hash = hash * 31 + s.GetHashCode();
        return hash;
    }
}

public class EnemyData
{
    public List<State> StateHistory { get; } = new List<State>();
    public Dictionary<StateSequence, TransitionSegmentTree> NgramTree { get; } = new Dictionary<StateSequence, TransitionSegmentTree>();
    public List<int> Type { get; set; } = new List<int> { 5, 0 };
    public bool HasPrevious { get; set; } = false;
    public bool IsAlive { get; set; } = true;
    public double LastDirection { get; set; }
    public double LastX { get; set; }
    public double LastY { get; set; }
    public double LastEnergy { get; set; }
    public double LastSpeed { get; set; }
}

public struct Bullet
{
    public double X;
    public double Y;
    public double Speed;
    public double Direction;
    public double Power;
}

public class MyBullet
{
    public Bullet BulletData;
    public int Target;
    public int Type;

    public MyBullet(double x, double y, double speed, double direction, double power, int target, int type)
    {
        BulletData = new Bullet { X = x, Y = y, Speed = speed, Direction = direction, Power = power };
        Target = target;
        Type = type;
    }
}

public class Line2D
{
    public double X1 { get; }
    public double Y1 { get; }
    public double X2 { get; }
    public double Y2 { get; }

    public Line2D(double x1, double y1, double x2, double y2)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
    }

    public double DistanceToPoint(double px, double py)
    {
        return Math.Abs((Y2 - Y1) * px - (X2 - X1) * py + (X2 * Y1 - Y2 * X1)) 
                / Math.Sqrt(Math.Pow(Y2 - Y1, 2) + Math.Pow(X2 - X1, 2));
    }
}

public class TransitionSegmentTree
{
    private List<KeyValuePair<State, int>> data;
    private int size;
    private (State state, int frequency)[] tree;
    private Dictionary<State, int> stateToIndex;

    public TransitionSegmentTree()
    {
        data = new List<KeyValuePair<State, int>>();
        stateToIndex = new Dictionary<State, int>();
        size = 0;
        tree = new (State, int)[0];
    }

    public void Add(State s)
    {
        if (stateToIndex.ContainsKey(s))
        {
            int idx = stateToIndex[s];
            var kvp = data[idx];
            data[idx] = new KeyValuePair<State, int>(s, kvp.Value + 1);
        }
        else
        {
            stateToIndex[s] = data.Count;
            data.Add(new KeyValuePair<State, int>(s, 1));
        }
        RebuildTree();
    }

    private void RebuildTree()
    {
        int n = data.Count;
        if (n == 0)
        {
            tree = new (State, int)[0];
            size = 0;
            return;
        }
        size = 1;
        while (size < n) size *= 2;
        tree = new (State, int)[2 * size];
        for (int i = 0; i < size; i++)
        {
            if (i < n)
            {
                tree[size + i] = (data[i].Key, data[i].Value);
            }
            else
            {
                tree[size + i] = (default(State), 0);
            }
        }
        for (int i = size - 1; i > 0; i--)
        {
            var left = tree[2 * i];
            var right = tree[2 * i + 1];
            tree[i] = left.frequency >= right.frequency ? left : right;
        }
    }

    public State GetMostFrequent()
    {
        if (tree.Length > 0)
        {
            return tree[1].state;
        }
        return default(State);
    }
}