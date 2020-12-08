<Query Kind="Program" />

void Main(string[] args)
{
	var inputPath = @".\input.txt";
	if (args?.Length > 0 && File.Exists(args[0]))
		inputPath = args[0];
		
	var program = ReadLines(inputPath).ToList();
	var handheldConsole = new HandheldConsole(program);
	
	handheldConsole.RunUntilCompletion();
	handheldConsole.GlobalAcc.Dump("Solution 1");
	
	GenerateProgramVariations(program)
		.Select(program => new HandheldConsole(program))
		.First(console =>
			console.RunUntilCompletion() == HandheldConsole.HaltCause.CompletedOk)
		.GlobalAcc
		.Dump("Solution 2");
}

IEnumerable<List<string>> GenerateProgramVariations(List<string> program)
{
	for (int i = 0; i < program.Count; i++)
	{
		if (program[i].StartsWith("acc"))
			continue;
	
		var result = new List<string>();

		result.AddRange(program.Take(i));
		result.Add(program[i].StartsWith("n")
			? program[i].Replace("nop", "jmp")
			: program[i].Replace("jmp", "nop"));
		result.AddRange(program
			.Skip(i + 1)
			.Take(program.Count));
			
		yield return result;
	}
}

class HandheldConsole
{
	public enum HaltCause
	{
		CompletedOk,
		ProgramCounterOutsideOfBounds,
		LoopDetected
	}

	public int GlobalAcc { get; private set; }
	
	public HandheldConsole(List<string> program)
	{
		GlobalAcc = 0;
		mProgramCounter = 0;

		mProgram = program;
		mVisitedInstructions = new HashSet<int>();
	}
	
	public HaltCause RunUntilCompletion()
	{
		while (!mVisitedInstructions.Contains(mProgramCounter))
		{
			if (mProgramCounter < 0 || mProgramCounter >= mProgram.Count)
				return HaltCause.ProgramCounterOutsideOfBounds;

			mVisitedInstructions.Add(mProgramCounter);
			string instruction = mProgram[mProgramCounter];
			
			string opCode = instruction.Substring(0, 3);
			string opArgs = instruction.Substring(4);

			int nextPcIncrement = 1;
			switch (opCode)
			{
				case "nop":
					break;
					
				case "acc":
					GlobalAcc += ParseVal(opArgs);
					break;
					
				case "jmp":
					nextPcIncrement = ParseVal(opArgs);
					break;
					
				default:
					break;
			}
			
			mProgramCounter += nextPcIncrement;

			if (mProgramCounter == mProgram.Count)
				return HaltCause.CompletedOk;
		}
		
		return HaltCause.LoopDetected;
	}
	
	int ParseVal(string value)
	{
		int n = int.Parse(value.Substring(1));
		return (value[0] == '-') ? n * -1 : n;
	}
	
	int mProgramCounter;
	
	readonly List<string> mProgram;
	readonly HashSet<int> mVisitedInstructions;
}

IEnumerable<string> ReadLines(string path)
{
	using FileStream fs = File.OpenRead(path);
	using StreamReader sr = new StreamReader(fs);
		
	string line;
	while ((line = sr.ReadLine()) != null)
		yield return line;
}
