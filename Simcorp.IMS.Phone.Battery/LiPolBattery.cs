using System;

namespace Simcorp.IMS.Phone.Battery {
    public class LiPolBattery : BaseBattery, ICharge, IGiveCharge {
        public LiPolBattery(double vol, double curChar) : base(vol, curChar) {}

        public override string ToString() {
            return "Lithium polymer battery: " + this.Capacity + " mAh";
        }
    }
}
