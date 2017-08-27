﻿using System;
using System.Reflection;
using UnityEngine;

namespace CharlesOlinerCommandConsole {
    public class ConsoleCommand : MonoBehaviour {

        //wrapper class for commands to give them more functions.
        //you can add more functions here if you want, but they will only show up in the auto-type list if you add it in CommandConsole.cs.
        //(they also won't be usable if they're private and will be very difficult to use if they're not static.)

        public static void members(Type t) {
            foreach (MemberInfo member in t.GetMembers()) {
                switch (member.MemberType) {
                    case MemberTypes.Constructor:
                        ConstructorInfo c = (ConstructorInfo)member;
                        if (c.IsPrivate) continue;
                        break;
                    case MemberTypes.Custom: break; //no way to check whether acessible
                    case MemberTypes.Event:
                        EventInfo e = (EventInfo)member;
                        if (e.GetAddMethod().IsPrivate) {
                            if (!e.GetRaiseMethod().IsPrivate) {
                                if (!e.GetRemoveMethod().IsPrivate) {
                                    if (e.GetOtherMethods(false).Length == 0) continue;
                                }
                            }
                        }
                        break;
                    case MemberTypes.Field:
                        FieldInfo f = (FieldInfo)member;
                        if (f.IsPrivate) continue;
                        break;
                    case MemberTypes.Method:
                        MethodInfo m = (MethodInfo)member;
                        if (m.IsSpecialName) {
                            if (m.Name.StartsWith("set_")) continue; //removes set accessors (which cannot be invoked directly)
                            if (m.Name.StartsWith("get_")) continue; //removes get accessors (which cannot be invoked directly)
                        }
                        if (m.IsPrivate) continue;
                        break;
                    case MemberTypes.NestedType: //can't figure out how to check this for publicness. but it might be included in Assembly.GetTypes();
                        break;
                    case MemberTypes.Property:
                        PropertyInfo p = (PropertyInfo)member;
                        if (p.GetAccessors(false).Length == 0) continue;
                        break;
                    case MemberTypes.TypeInfo: //can't figure out how to check this for publicness either. but it too might be included in Assembly.GetTypes();
                        break;
                }
                Debug.Log(member.MemberType + " " + member.Name);
            }
        }
        public static void members(object o) {
            members(o.GetType());
        }

        public static GameObject summon(string resourceName) {
            if (resourceName == null) {
                Debug.LogError("summon(): Resource name cannot be null.");
                return null;
            }
            object o = Resources.Load(resourceName);
            if (o == null) {
                Debug.LogError("summon(): Could not find " + resourceName + " in resources folder.");
                return null;
            }
            if (o.GetType() != typeof(GameObject)) {
                Debug.LogError("summon(): " + resourceName + " is not a GameObject.");
                return null;
            }
            RaycastHit h;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out h)) {
                return GameObject.Instantiate((GameObject)o, h.point, Camera.main.transform.rotation);
            } else {
                Debug.LogWarning("summon(): Mouse pointer is not over a location to summon at. Summoning at main camera.");
                return GameObject.Instantiate((GameObject)o, Camera.main.transform.position, Camera.main.transform.rotation);
            }
        }

        public static void relocate(GameObject relocatee) {
            if (relocatee == null) {
                Debug.LogError("relocate(): Target is null.");
                return;
            }
            Rigidbody r = relocatee.GetComponent<Rigidbody>();
            if (r != null) {
                r.AddForce(-r.velocity, ForceMode.VelocityChange);
                r.AddTorque(-r.angularVelocity, ForceMode.VelocityChange);
            }
            RaycastHit h;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out h)) {
                relocatee.transform.position = h.point + h.normal * relocatee.transform.localScale.y;
            } else {
                Debug.LogError("relocate(): Mouse pointer is not over a location to teleport to.");
            }
        }

        public static void push(GameObject pushee) {
            push(pushee, Camera.main.transform.forward * 5);
        }
        public static void push(GameObject pushee, float pushSpeed) {
            push(pushee, Camera.main.transform.forward * pushSpeed);
        }

        public static void push(GameObject pushee, Vector3 pushVec) {
            if (pushee == null) {
                Debug.LogError("push(): Target is null.");
                return;
            }
            Rigidbody r = pushee.GetComponent<Rigidbody>();
            if (r == null) {
                Debug.LogError("push(): Target does not have a rigidbody.");
                return;
            }
            r.AddForce(pushVec, ForceMode.VelocityChange);
        }
    }
}
