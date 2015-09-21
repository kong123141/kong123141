using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace PentakillOrianna.Util {
    class Ball {

        Vector3 ballPosition;

        public Ball(Vector3 ballPosition) {
            this.ballPosition = ballPosition;
        }

        public Vector3 getPosition() {
            return ballPosition;
        }

        public void setPosition(Vector3 ballPosition) {
            this.ballPosition = ballPosition;
        }
    }
}
