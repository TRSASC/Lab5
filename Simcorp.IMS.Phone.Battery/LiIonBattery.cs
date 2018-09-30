using System;

namespace Simcorp.IMS.Phone.Battery {
    public class LiIonBattery : BaseBattery, ICharge, IGiveCharge {
        public LiIonBattery(double vol, double curChar) : base(vol, curChar) { }

        public override string ToString() {
            return "Lithium-ion battery: " + this.Capacity + " mAh";
        }
    }
}