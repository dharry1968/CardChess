# App.Agent

An *Agent* represents a *Model*.

It acts as the intermediary between the *Arbitrator* and the rest of the system.

Models contain just state. They are persistent and can be sent over the network.

An _Agent_ however is ephemeral and transient. It is based on _Flow.Transient_ and can perform actions over time.

Agents work over time; Models are just static state.

All Requests to change state of a Model are forwarded through an Agent. 

Agents are *not* MonoBehaviours. Only *Views* are MonoBehaviors.

