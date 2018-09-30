using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simcorp.IMS.Phone.Battery;

namespace TaskTest {
    [TestClass]
    public class ChargeTest {
        public void DummyMethod() { }

        [TestMethod]
        public void MaxChargeTest() {
            double capacity = 3000;
            BaseBattery battery = new LiIonBattery(capacity, 1500);
            battery.ChargeChanged += DummyMethod;

            battery.Charge(capacity+1);

            Assert.AreEqual(100, battery.GetCurrentCharge());
        }

        [TestMethod]
        public void MinChargeTest() {
            double capacity = 3000;
            BaseBattery battery = new LiIonBattery(capacity, 1500);
            battery.ChargeChanged += DummyMethod;

            battery.Discharge(capacity+1);

            Assert.AreEqual(0, battery.GetCurrentCharge());
        }

        [TestMethod]
        public void TaskChargeTest() {
            double capacity = 3000;
            BaseBattery battery = new LiIonBattery(capacity, 1500);
            battery.ChargeChanged += DummyMethod;
            double expCharge = 52;
            Task charge = new Task(() => {
                battery.Charge(50);
            }
            );

            charge.Start();
            charge.Wait();
            double resCharge = battery.GetCurrentCharge();

            Assert.AreEqual(expCharge, resCharge);
        }

        [TestMethod]
        public void TaskDischargeTest() {
            double capacity = 3000;
            BaseBattery battery = new LiIonBattery(capacity, 1500);
            battery.ChargeChanged += DummyMethod;
            double expCharge = 48;
            Task charge = new Task(() => {
                battery.Discharge(50);
            }
            );

            charge.Start();
            charge.Wait();
            double resCharge = battery.GetCurrentCharge();

            Assert.AreEqual(expCharge, resCharge);
        }

    }
}
