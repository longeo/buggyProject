## Senior Freshman Engineering Buggy Project. 
Goal: Build autonomous vehicles capable of interacting with surroundings. Gold medal awarded project.

This report will cover the design and implementation of a pair of autonomous vehicles (buggies),
controlled by a remote supervisor, under a specific set of rules. We will cover the technical aspects
of how this was achieved, as well as the practical steps we took as a team to complete the
required task.

We were supplied with buggies which had the sensors, motors, communications equipment and
power management circuitry required, although it was necessary to design a “Control Board”,
which contained the “Buggy Control Chip” (BCC), discrete line following logic and motor control.

The control board will be covered in detail later, but in short;
We completed a truth table using the four inputs (Left eye, Right eye, Left override, and Right
override) to ensure the motors would be powered in such a way that line following would be
achieved.
This truth table was converted to a simplified Boolean expression.
It was then converted to a logic circuit using just two type of gates.(The actual limitation was no
more than two logic chips, it could have been achieved with a single type of logic gate)
This logical circuit was then used to design an actual board layout(which included the h­bridge and
BCC)
This control board was then able to power the motors and achieve line following if the BCC was
send a command to enable to the motors and enter normal line following mode.

In brief the buggies were required to;
Follow a line which represented the track. This was achieved using two IR emitter sensors pairs
attached to a comparator which passed the relevant digital signal to the control board which
contained the line following logic.
Avoid Collisions with either obstacles or other buggies. This was achieved using a Ultrasonic
distance module attached to the arduino, which could be pinged to find the distance from the
front of the buggy to the nearest object(in front of the buggy).
Stop at each “Gantry” (a gate which separate each section of the track). A section of the track
could only contain a single buggy at a time. As such, the supervisor stores the section which each
buggy is currently located, and used this information to either order the buggy to wait or proceed
when it reaches a gantry.
“Park” in the parking bay. This manoeuvre was completed by manipulating the line following logic
(using a command sent to the Buggy Control Chip, more details below), such that the buggy
followed the line into the parking bay, and then using the Ultrasonic Distance Sensor to detect a
gantry in front of the parking bay and thus determine when it is required to stop.

Rotate in the parking bay to face the reverse direction. This manoeuvre was again completed by
manipulating the line following logic as above.
Complete laps in reverse order, ignoring the rule for only one buggy per section. This required a
change to the supervisor code which indicated it should allow buggies to proceed at gantries even
if there was a buggy in the following section. It also required that we set a “follow” mode on the
arduino, which involved the read buggy to increase or decrease speed based on its distance from
the buggy in front of it, and still stop if it became too close.
“Park” in the reverse direction. This was completed as before but using a different command.

These required tasks determined certain requirements, one of which was wireless communication.
This was achieved using;
An xbee on the buggy
An xbee on the supervisor
Code written on both the buggy and the supervisor to ensure the xbees were configured correctly
Code written on both the buggy and the supervisor to ensure that the messages received by both
were parsed and processed correctly.
