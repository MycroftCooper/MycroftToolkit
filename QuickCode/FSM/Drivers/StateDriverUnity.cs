﻿
using UnityEngine;

namespace MycroftToolkit.QuickTool.FSM {
    public class StateDriverUnity {
        public StateEvent Awake;
        public StateEvent LateUpdate;
        public StateEvent<int> OnAnimatorIK;
        public StateEvent OnAnimatorMove;
        public StateEvent<bool> OnApplicationFocus;
        public StateEvent OnApplicationPause;
        public StateEvent OnApplicationQuit;
        public StateEvent<float[], int> OnAudioFilterRead;
        public StateEvent OnBecameInvisible;
        public StateEvent OnBecameVisible;
        public StateEvent<Collider> OnCollisionEnter;
        public StateEvent<Collision2D> OnCollisionEnter2D;
        public StateEvent<Collider> OnCollisionExit;
        public StateEvent<Collider2D> OnCollisionExit2D;
        public StateEvent<Collision> OnCollisionStay;
        public StateEvent<Collision2D> OnCollisionStay2D;
        public StateEvent OnConnectedToServer;
        public StateEvent<ControllerColliderHit> OnControllerColliderHit;
        public StateEvent OnDestroy;
        public StateEvent OnDisable;
        public StateEvent OnDrawGizmos;
        public StateEvent OnDrawGizmosSelected;
        public StateEvent OnEnable;
        public StateEvent OnGUI;
        public StateEvent<float> OnJointBreak;
        public StateEvent<Joint2D> OnJointBreak2D;
        public StateEvent OnMouseDown;
        public StateEvent OnMouseDrag;
        public StateEvent OnMouseEnter;
        public StateEvent OnMouseExit;
        public StateEvent OnMouseOver;
        public StateEvent OnMouseUp;
        public StateEvent OnMouseUpAsButton;
        public StateEvent<GameObject> OnParticleCollision;
        public StateEvent OnParticleSystemStopped;
        public StateEvent OnParticleTrigger;
        public StateEvent OnPostRender;
        public StateEvent OnPreCull;
        public StateEvent<RenderTexture, RenderTexture> OnRenderImage;
        public StateEvent OnRenderObject;
        public StateEvent OnTransformChildrenChanged;
        public StateEvent OnTransformParentChanged;
        public StateEvent<Collider> OnTriggerEnter;
        public StateEvent<Collider2D> OnTriggerEnter2D;
        public StateEvent<Collider> OnTriggerExit;
        public StateEvent<Collider2D> OnTriggerExit2D;
        public StateEvent<Collider> OnTriggerStay;
        public StateEvent<Collider2D> OnTriggerStay2D;
        public StateEvent OnValidate;
        public StateEvent OnWillRenderOjbect;
        public StateEvent Reset;
        public StateEvent Start;
        public StateEvent Update;

        //Unity Networking Deprecated
        //public StateEvent<NetworkDisconnection> OnDisconnectedFromServer;
        //public StateEvent<NetworkConnectionError> OnFailedToConnect;
        //public StateEvent<NetworkConnectionError> OnFailedToConnectToMasterServer;
        //public StateEvent<MasterServerEvent> OnMasterServerEvent;
        //public StateEvent<NetworkMessageInfo> OnNetworkInstantiate;
        //public StateEvent<NetworkPlayer> OnPlayerConnected;
        //public StateEvent<NetworkPlayer> OnPlayerDisconnected;
        //public StateEvent<BitStream, NetworkMessageInfo> OnSerializeNetworkView;
        //public StateEvent OnSeverInitialized;

    }
}
