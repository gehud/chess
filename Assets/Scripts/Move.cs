namespace Chess {
	public readonly struct Move {
		public readonly int From;
		public readonly int To;

		public Move(int from, int to) {
			From = from;
			To = to;
		}
	}
}