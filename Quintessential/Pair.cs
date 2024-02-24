namespace Quintessential;

public class Pair<A, B> {

	public Pair(){}

	public Pair(A left, B right) {
		Left = left;
		Right = right;
	}

	public A Left {
		get; set;
	}

	public B Right {
		get; set;
	}
}
