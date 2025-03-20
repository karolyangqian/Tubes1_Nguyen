using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// diem
// ------------------------------------------------------------------
// Targetting: None
// Movement: None
// ------------------------------------------------------------------
public class Diem : Bot
{   
    private Random random = new Random();
    
    static void Main(string[] args)
    {
        new Diem().Start();
    }

    Diem() : base(BotInfo.FromFile("DiemL.json")) { }

    public override void Run()
    {
        AdjustRadarForBodyTurn = true;
        AdjustGunForBodyTurn = true;
        SetTurnRadarRight(double.PositiveInfinity);
    }

    public override void OnTick(TickEvent e)
    {
        BodyColor = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
        TurretColor = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
        RadarColor = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
        BulletColor = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
        ScanColor = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        double firePower = 3;
        if (GunTurnRemaining == 0)
        {
            Fire(firePower);
        }

        // Head on
        // TurnGunLeft(GunBearingTo(e.X, e.Y));

        // Linear
        double bulletSpeed = CalcBulletSpeed(firePower);
        
        double dx = e.X - X;
        double dy = e.Y - Y;
        
        double enemyDir = e.Direction * Math.PI / 180.0;
        
        double distance = Math.Sqrt(dx * dx + dy * dy);
        double time = distance / bulletSpeed;
        
        double predictedX = e.X + e.Speed * time * Math.Cos(enemyDir);
        double predictedY = e.Y + e.Speed * time * Math.Sin(enemyDir);
        
        TurnGunLeft(GunBearingTo(predictedX, predictedY));
    }
}
