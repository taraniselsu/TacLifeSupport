namespace RSTKSPGameEvents
{
    public static class RSTEvents
    {
        /// <summary>
        /// Fires when DeepFreeze Completes the Freezing process on a Kerbal.
        /// Part is the DeepFreeze Freezer Part and ProtoCrewMember is the Kerbal.
        /// </summary>
        public static EventData<Part, ProtoCrewMember> onKerbalFrozen = new EventData<Part, ProtoCrewMember>("onKerbalFrozen");
        /// <summary>
        /// Fires when DeepFreeze Completes the Thawing process on a Kerbal.
        /// Part is the DeepFreeze Freezer Part and ProtoCrewMember is the Kerbal.
        /// </summary>
        public static EventData<Part, ProtoCrewMember> onKerbalThaw = new EventData<Part, ProtoCrewMember>("onKerbalThaw");
        /// <summary>
        /// Fires when DeepFreeze sets a Kerbal to Comatose Status.
        /// Part is the DeepFreeze Freezer Part and ProtoCrewMember is the Kerbal.
        /// </summary>
        public static EventData<Part, ProtoCrewMember> onKerbalSetComatose = new EventData<Part, ProtoCrewMember>("onKerbalSetComatose");
        /// <summary>
        /// Fires when DeepFreeze Unsets a Kerbal from Comatose Status.
        /// Part is the DeepFreeze Freezer Part and ProtoCrewMember is the Kerbal.
        /// </summary>
        public static EventData<Part, ProtoCrewMember> onKerbalUnSetComatose = new EventData<Part, ProtoCrewMember>("onKerbalUnSetComatose");
        /// <summary>
        /// Fires when DeepFreeze has to Kill a Frozen Kerbal.
        /// </summary>
        public static EventData<ProtoCrewMember> onFrozenKerbalDied = new EventData<ProtoCrewMember>("onFrozenKerbalDied");


        /// <summary>
        /// Fires when ResearchBodies changes the Visibility of a CelestialBody.
        /// CelestialBody is the body, and int is the Visibility Level. 
        /// </summary>
        public static EventData<CelestialBody, int> onSetCelestialBodyVisibility = new EventData<CelestialBody, int>("onSetCelestialBodyVisibility");
        /// <summary>
        /// Fires when a new Celestial Body is Found in ResearchBodies. 
        /// </summary>
        public static EventData<CelestialBody> onCelestialBodyFound =new EventData<CelestialBody>("onCelestialBodyFound");
        /// <summary>
        /// Fires when a Photo is taken of a CelestialBody by Tarsier Space Technology Telescope
        /// </summary>
        public static EventData<CelestialBody> onCelestialBodyPhoto = new EventData<CelestialBody>("onCelestialBodyPhoto");
        /// <summary>
        /// Fires when a Celestial Body is found by a ResearchBodies Scanner
        /// </summary>
        public static EventData<CelestialBody> onCelestialBodyScanned = new EventData<CelestialBody>("onCelestialBodyScanned");
    }
}
