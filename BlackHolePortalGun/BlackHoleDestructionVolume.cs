namespace BlackHolePortalGun
{
	public class ModifiedBlackHoleDestructionVolume : DestructionVolume
	{
        public override void Awake()
        {
			base.Awake();
        }

		private void Start()
		{

		}
		public  void setWhiteHole(WhiteHoleVolume hole)
		{
			this._whiteHole = hole;
		}
		public WhiteHoleVolume getWhiteHole()
		{
			return this._whiteHole;
		}

		public override void VanishPlayer(OWRigidbody playerBody, RelativeLocationData entryLocation)
		{
			if (PlayerState.IsInsideShip() || PlayerState.IsInsideShuttle())
			{
				return;
			}

			GlobalMessenger.FireEvent("PlayerEnterBlackHole");
			this._whiteHole.ReceiveWarpedBody(playerBody, entryLocation);
		}


		public WhiteHoleVolume _whiteHole;
	}
}

