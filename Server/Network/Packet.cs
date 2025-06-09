public class Packet
{
    public string Type { get; set; } = "";
    public string Payload { get; set; } = "";

    public static Packet Parse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new ArgumentException("빈 메시지입니다.");

        string[] split = raw.Split(' ', 2);
        return new Packet { Type = split[0], Payload = split.Length > 1 ? split[1] : "" };
    }

    public override string ToString() => $"{Type} {Payload}";
}
