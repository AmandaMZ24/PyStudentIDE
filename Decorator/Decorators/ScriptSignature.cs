using System;

namespace DecoratorApp.Decorators;

public record ScriptSignature(string ScriptPath, string Hash, DateTime SignedAt);
