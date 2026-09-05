using Raccoon.Controller;
using Raccoon.EnumHolder;
using Raccoon.UI;
using UnityEngine;
using UnityEngine.Events;


namespace Raccoon.Island
{

    public class ShopGO : BaseGOTrigerContact
    {
        public TypeShop type;
        protected override void Start()
        {
            base.Start();
        }
        public override void OnInteract()
        {
            
        }
        protected override void WaitDone(Transform contactor)
        {
            print("Wait done " + contactor.name);
            contactor.GetComponent<ICharactor>()?.ContactShop(type);
        }

    }
}
