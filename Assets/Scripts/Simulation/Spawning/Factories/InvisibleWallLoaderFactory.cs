using UnityEngine;

class InvisibleWallFactory : MonoBehaviourFactory{
    public static InvisibleWallLoader Create(BoxCollider wall1, BoxCollider wall2, BoxCollider wall3, BoxCollider wall4, GameObject gameObject = null){
        InvisibleWallLoader iwl = Create<InvisibleWallLoader>(gameObject);
        iwl.wall1 = wall1;
        iwl.wall2 = wall2;
        iwl.wall3 = wall3;
        iwl.wall4 = wall4;
        return iwl;
    }
}