using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// Miaww
// ------------------------------------------------------------------
// Targeting: Linear Targteting
// Movement: Random
// ------------------------------------------------------------------
public class Miaww : Bot
{
    bool movingForward;

    static void Main()
    {
        new Miaww().Start();
    }

    Miaww() : base(BotInfo.FromFile("miaww.json")) { }

    public override void Run()
    {
        BodyColor = Color.Black;
        TurretColor = Color.Black;
        RadarColor = Color.Black;
        BulletColor = Color.Black;
        ScanColor = Color.Black;

        movingForward = true;
        SetTurnRadarRight(double.PositiveInfinity);
        AdjustGunForBodyTurn = true;

        while (IsRunning)
        {
            SetForward(40000);
            movingForward = true;
            SetTurnRight(90);
            WaitFor(new TurnCompleteCondition(this));
            SetTurnLeft(180);
            WaitFor(new TurnCompleteCondition(this));
            SetTurnRight(180);
            WaitFor(new TurnCompleteCondition(this));
        }
    }

    public override void OnHitWall(HitWallEvent e)
    {
        ReverseDirection();
    }

    public void ReverseDirection()
    {
        if (movingForward)
        {
            BodyColor = Color.Black;
            SetBack(40000);
            movingForward = false;
        }
        else
        {
            BodyColor = Color.White;
            SetForward(40000);
            movingForward = true;
        }
    }

public override void OnScannedBot(ScannedBotEvent e)
{
    double firePower = 1;
    double bulletSpeed = CalcBulletSpeed(firePower);
    
    double dx = e.X - X;
    double dy = e.Y - Y;
    double absBearing = Math.Atan2(dy, dx);
    
    double enemyDir = e.Direction * Math.PI / 180.0;
    
    double ratio = Math.Max(-1, Math.Min(1, (e.Speed * Math.Sin(enemyDir - absBearing)) / bulletSpeed));
    double leadAngle = Math.Asin(ratio);
    
    double gunDirection = absBearing + leadAngle;
    
    double distance = Math.Sqrt(dx * dx + dy * dy);
    double time = distance / bulletSpeed;
    
    double predictedX = e.X + e.Speed * time * Math.Cos(enemyDir);
    double predictedY = e.Y + e.Speed * time * Math.Sin(enemyDir);
    
    double bearingFromGun = GunBearingTo(predictedX, predictedY);
    
    TurnGunLeft(bearingFromGun);
    Fire(firePower);
}

    public override void OnHitBot(HitBotEvent e)
    {
        if (e.IsRammed)
        {
            ReverseDirection();
        }
    }

    public override void OnWonRound(WonRoundEvent e)
    {
        // Victory dance turning right 360 degrees 100 times
        TurnLeft(36_000);
    }
}

public class TurnCompleteCondition : Condition
{
    private readonly Bot bot;

    public TurnCompleteCondition(Bot bot)
    {
        this.bot = bot;
    }

    public override bool Test()
    {
        return bot.TurnRemaining == 0;
    }
}