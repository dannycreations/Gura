using System;

public struct Vector2i
{
	public Vector2i(short x0, short y0)
	{
		this.x = x0;
		this.y = y0;
	}

	public static Vector2i operator +(Vector2i left, Vector2i right)
	{
		return new Vector2i(left.x + right.x, left.y + right.y);
	}

	public static Vector2i operator -(Vector2i left, Vector2i right)
	{
		return new Vector2i(left.x - right.x, left.y - right.y);
	}

	public static bool operator ==(Vector2i left, Vector2i right)
	{
		return left.x == right.x && left.y == right.y;
	}

	public static bool operator !=(Vector2i left, Vector2i right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		return obj is Vector2i && (Vector2i)obj == this;
	}

	public override int GetHashCode()
	{
		return (int)this.y * 100000 + (int)this.x;
	}

	public override string ToString()
	{
		return string.Format("(y = {0}, x = {1})", this.y, this.x);
	}

	public short x;

	public short y;
}
