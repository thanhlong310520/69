using Raccoon.Building;
using Raccoon.EnumHolder;
using Raccoon.Item;
using UnityEngine;
namespace Raccoon.Controller
{

    public interface InteractObjectByTrigger
    {
        void HandleStartInteract(Transform contactor);
        void HandleStopInteract(Transform contactor);
        void Raise();
    }

    public interface IReceive
    {
        void KnockBack(int value, Transform sender);
    }

    public interface IIteractSlot
    {
        void Income(long income);  
    }

    public interface ICharactor
    {
        void SetCharactorData(CharactorData data);
        void SetStartPoint(Vector3 startPoint);
        void ContactShop(TypeShop type);
        void ContactWave();
        void Stealed(SlotController slot);
        void NotifyBossChase();
    }
}