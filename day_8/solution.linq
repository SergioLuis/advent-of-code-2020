<Query Kind="Program" />

void Main(string[] args)
{
	var inputPath = @".\input.txt";
	if (args?.Length > 0 && File.Exists(args[0]))
		inputPath = args[0];
		
	var instructions = ReadLines(inputPath).ToList();
	var handheldConsole = new HandheldConsole(instructions);
	
	handheldConsole.RunUntilCompletion();
	handheldConsole.GlobalAcc.Dump("Solution 1");
	
	GenerateProgramVariations(instructions)
		.Select(program => new HandheldConsole(program))
		.First(console => console.RunUntilCompletion())
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
		
		for (int j = 0; j < i; j++)
			result.Add(program[j]);
			
		result.Add(program[i].StartsWith("nop")
			? program[i].Replace("nop", "jmp")
			: program[i].Replace("jmp", "nop"));
		
		for (int j = i + 1; j < program.Count; j++)
			result.Add(program[j]);
			
		yield return result;
	}
}

IEnumerable<string> ReadLines(string path)
{
	using FileStream fs = File.OpenRead(path);
	using StreamReader sr = new StreamReader(fs);
		
	string line;
	while ((line = sr.ReadLine()) != null)
		yield return line;
}

class HandheldConsole
{
	public int ProgramCounter { get; private set; }
	public int GlobalAcc { get; private set; }
	
	public HandheldConsole(List<string> program)
	{
		ProgramCounter = 0;
		GlobalAcc = 0;
		mProgram = program;
		mVisitedInstructions = new HashSet<int>();
	}
	
	public bool RunUntilCompletion()
	{
		while (!mVisitedInstructions.Contains(ProgramCounter))
		{
			if (ProgramCounter < 0 || ProgramCounter >= mProgram.Count)
				return false;

			mVisitedInstructions.Add(ProgramCounter);
			string instruction = mProgram[ProgramCounter];
			
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
			
			ProgramCounter += nextPcIncrement;
			
			// ProgramCounter set to next instruction in init code
			// Valid run
			if (ProgramCounter == mProgram.Count)
				return true;
		}
		
		// Loop detected
		// Invalid run
		return false;
	}
	
	int ParseVal(string value)
	{
		int n = int.Parse(value.Substring(1));
		return (value[0] == '-')
			? n * -1
			: n;
	}
	
	readonly List<string> mProgram;
	readonly HashSet<int> mVisitedInstructions;
}