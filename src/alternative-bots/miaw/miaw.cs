using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// Miaw
// ------------------------------------------------------------------
// Targetting: Naive
// Movement: Random
// ------------------------------------------------------------------
public class Miaw : Bot
{
    bool movingForward;

    static void Main()
    {
        new Miaw().Start();
    }

    Miaw() : base(BotInfo.FromFile("miaw.json")) { }

    public override void Run()
    {
        BodyColor = Color.FromArgb(0x00, 0xC8, 0x00);   // lime
        TurretColor = Color.FromArgb(0x00, 0x96, 0x32); // green
        RadarColor = Color.FromArgb(0x00, 0x64, 0x64);  // dark cyan
        BulletColor = Color.FromArgb(0xFF, 0xFF, 0x64); // yellow
        ScanColor = Color.FromArgb(0xFF, 0xC8, 0xC8);   // light red

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
        var bearingFromGun = GunBearingTo(e.X, e.Y);

        TurnGunLeft(bearingFromGun);
        Fire(1);

        SetTurnRadarLeft(RadarTurnRemaining);
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
        // TurnRadarRight(RadarTurnRemaining);
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