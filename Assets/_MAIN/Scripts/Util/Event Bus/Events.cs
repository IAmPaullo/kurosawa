using System;
using System.Collections.Generic;
using UnityEngine;

public interface IEvent { }
public struct Events : IEvent { }
public struct EventsWithArgument : IEvent
{
    public bool isEvent;
}
