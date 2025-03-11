using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// diem
// ------------------------------------------------------------------
// Targeting: None
// Movement: None
// ------------------------------------------------------------------
public class Diem : Bot
{   
    private Random random = new Random();

    private double distance = double.PositiveInfinity;
    private int id;
    
    static void Main(string[] args)
    {
        new Diem().Start();
    }

    Diem() : base(BotInfo.FromFile("Diem.json")) { }

    public override void Run()
    {
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
        Console.WriteLine("I see a bot!");

        if (DistanceTo(e.X, e.Y) < distance || e.ScannedBotId == id)
        {
            distance = DistanceTo(e.X, e.Y);
            id = e.ScannedBotId;
        
            if (GunHeat < 1) 
            {
                SetTurnRadarLeft(double.PositiveInfinity * NormalizeRelativeAngle(RadarBearingTo(e.X, e.Y)));
            } 

            if (GunTurnRemaining == 0)
            {
                SetFire(1);
                distance = double.PositiveInfinity;
            }

            SetTurnGunLeft(GunBearingTo(e.X, e.Y));
        }
    }
}
