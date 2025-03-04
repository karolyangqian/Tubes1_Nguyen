import dev.robocode.tankroyale.botapi.*;
import dev.robocode.tankroyale.botapi.events.*;

import java.awt.*;

// ---------------------------------------------------------------------------
// PaintingBot
// ---------------------------------------------------------------------------
// A sample bot original made for Robocode by Pavel Savara based on MyFirstBot
// Ported to Robocode Tank Royale by Flemming N. Larsen.
//
// A sample bot that demonstrate how to paint stuff on the battlefield.
// Remember to enable Graphical Debugging for the bot when running a battle.
// ---------------------------------------------------------------------------
public class PaintingBot extends Bot {

    double scannedX;
    double scannedY;
    int scannedTime;

    // The main method starts our bot
    public static void main(String[] args) {
        new PaintingBot().start();
    }

    // Constructor, which loads the bot config file
    PaintingBot() {
        super(BotInfo.fromFile("PaintingBot.json"));
    }

    // Called when a new round is started -> initialize and do some movement
    @Override
    public void run() {
        // Continuous forward and backward movement repeating forever
        while (isRunning()) {
            forward(100);
            turnGunRight(360);
            back(100);
            turnGunRight(360);
        }
    }

    // We saw another bot -> save the coordinates of the scanned bot and turn (time) when scanned
    @Override
    public void onScannedBot(ScannedBotEvent e) {
        // Get the coordinates of the scanned bot and the time (turn) when scanned
        scannedX = (int) e.getX();
        scannedY = (int) e.getY();
        scannedTime = e.getTurnNumber();

        // Also, fire the gun!
        fire(1);
    }

    // During each turn (tick), we draw a red circle at the bot's last known location. We can't draw
    // the circle at the bot's current position because we need to scan it again to determine its
    // updated location.
    @Override
    public void onTick(TickEvent e) {
        // Check if we scanned a bot by checking if the scanned time is not 0
        if (scannedTime != 0) {

            // Calculate a color alpha value for transparency that.
            // The alpha value is at its maximum when a bot is initially scanned, gradually
            // diminishing over time as more time passes since the scan.
            int deltaTime = e.getTurnNumber() - scannedTime;
            int alpha = Math.max(0xff - (deltaTime * 16), 0);

            // Draw a red circle with the alpha value we calculated using anm ellipse
            var g = getGraphics();
            g.setColor(new Color(0xff, 0x00, 0x00, alpha));
            g.fillOval((int) scannedX - 20, (int) scannedY - 20, 40, 40);
        }
    }
}
