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
}
