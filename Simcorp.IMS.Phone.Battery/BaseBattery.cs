using System;

namespace Simcorp.IMS.Phone.Battery {
    public abstract class BaseBattery : ICharge, IGiveCharge {
        public delegate void CurrentChargeLevelChanged();
        public event CurrentChargeLevelChanged ChargeChanged;

        private double vCapacity;
        private double vChargeLevel;

        public double Capacity {
            get {
                return vCapacity;
            }
            private set {
                if (value <= 0) {throw new ArgumentOutOfRangeException("Battery capacity must be positive"); }
                vCapacity = value;
            }
        }

        protected double ChargeLevel {
            get {
                return vChargeLevel;
            }
            set {
                if (value < 0){
                    value = 0;
                }
                if (value > Capacity){
                    value = Capacity;
                }
                vChargeLevel = value;
            }
        }

        public int GetCurrentCharge() {
            return Convert.ToInt32((ChargeLevel* 100 / Capacity));
        }

        public BaseBattery(double vol, double curChar){
            Capacity = vol;
            ChargeLevel = curChar;
        }

        public void Charge(double energy) {
                this.ChargeLevel += energy;
                ChargeChanged();
        }

        public void Discharge(double energy) {
                ChargeLevel -= energy;
                ChargeChanged();
        }
    }
}