// Copyright (c) Matthias Wolf, Mawosoft.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CoverageDemo.Coverage;

// Exception blocks coverage.

public class ExceptionBlocks
{
    private readonly object _lock1 = new();
    private readonly Lock _lock2 = new();

    [ExcludeFromCodeCoverage]
    public static void Run()
    {
        for (int i = 0; i < 10; i++)
        {
            var c = new ExceptionBlocks();
            try { _ = c.UsingFull(-1); } catch { }
            try { _ = c.UsingFull(0); } catch { }
            try { _ = c.UsingPartial(0); } catch { }
            try { _ = c.MonitorLockPartial(0); } catch { }
            try { _ = c.LockLockPartial(0); } catch { }
            try { _ = c.TryCatchFull(null); } catch { }
            try { _ = c.TryCatchFull(new ArithmeticException()); } catch { }
            try { _ = c.TryCatchFull(new InvalidOperationException()); } catch { }
            try { _ = c.TryCatchFull(new FormatException()); } catch { }
            try { _ = c.TryCatchFull(new NotSupportedException("foo")); } catch { }
            try { _ = c.TryCatchFull(new NotSupportedException()); } catch { }
            try { _ = c.TryCatchNoException(null); } catch { }
            try { _ = c.TryCatchArithmeticException(new ArithmeticException()); } catch { }
            try { _ = c.TryCatchInvalidOperationException(new InvalidOperationException()); } catch { }
            try { _ = c.TryCatchFormatException(new FormatException()); } catch { }
            try { _ = c.TryCatchExceptionMessage(new NotSupportedException("foo")); } catch { }
            try { _ = c.TryCatchNotSupportedException(new NotSupportedException()); } catch { }
        }
    }

    public int UsingFull(MyInt value)
    {
        int result = 0;
        using (var sr = new StringReader(""))
        {
            ThrowIf(value >= 0);
            result |= 1;
        }
        return result;
    }

    public int UsingPartial(MyInt value)
    {
        int result = 0;
        using (var sr = new StringReader(""))
        {
            ThrowIf(value >= 0);
            result |= 1;
        }
        return result;
    }

    public int MonitorLockPartial(MyInt value)
    {
        int result = 0;
        lock (_lock1)
        {
            ThrowIf(value >= 0);
            result |= 1;
        }
        return result;
    }

    public int LockLockPartial(MyInt value)
    {
        int result = 0;
        lock (_lock2)
        {
            ThrowIf(value >= 0);
            result |= 1;
        }
        return result;
    }

    public int TryCatchFull(Exception? exception)
    {
        int result = 0;
        try
        {
            ThrowIf(exception);
            result |= 1;
        }
        catch (Exception ex) when (ex is ArithmeticException or ArgumentException)
        {
            result |= 2;
        }
        catch (InvalidOperationException)
        {
            result |= 4;
        }
        catch (SystemException ex) when (ex is FormatException || ex.Message == "foo")
        {
            result |= 8;
        }
        finally
        {
            result |= 0x10;
        }
        return result;
    }

    public int TryCatchNoException(Exception? exception)
    {
        int result = 0;
        try
        {
            ThrowIf(exception);
            result |= 1;
        }
        catch (Exception ex) when (ex is ArithmeticException or ArgumentException)
        {
            result |= 2;
        }
        catch (InvalidOperationException)
        {
            result |= 4;
        }
        catch (SystemException ex) when (ex is FormatException || ex.Message == "foo")
        {
            result |= 8;
        }
        finally
        {
            result |= 0x10;
        }
        return result;
    }

    public int TryCatchArithmeticException(Exception? exception)
    {
        int result = 0;
        try
        {
            ThrowIf(exception);
            result |= 1;
        }
        catch (Exception ex) when (ex is ArithmeticException or ArgumentException)
        {
            result |= 2;
        }
        catch (InvalidOperationException)
        {
            result |= 4;
        }
        catch (SystemException ex) when (ex is FormatException || ex.Message == "foo")
        {
            result |= 8;
        }
        finally
        {
            result |= 0x10;
        }
        return result;
    }

    public int TryCatchInvalidOperationException(Exception? exception)
    {
        int result = 0;
        try
        {
            ThrowIf(exception);
            result |= 1;
        }
        catch (Exception ex) when (ex is ArithmeticException or ArgumentException)
        {
            result |= 2;
        }
        catch (InvalidOperationException)
        {
            result |= 4;
        }
        catch (SystemException ex) when (ex is FormatException || ex.Message == "foo")
        {
            result |= 8;
        }
        finally
        {
            result |= 0x10;
        }
        return result;
    }

    public int TryCatchFormatException(Exception? exception)
    {
        int result = 0;
        try
        {
            ThrowIf(exception);
            result |= 1;
        }
        catch (Exception ex) when (ex is ArithmeticException or ArgumentException)
        {
            result |= 2;
        }
        catch (InvalidOperationException)
        {
            result |= 4;
        }
        catch (SystemException ex) when (ex is FormatException || ex.Message == "foo")
        {
            result |= 8;
        }
        finally
        {
            result |= 0x10;
        }
        return result;
    }

    public int TryCatchExceptionMessage(Exception? exception)
    {
        int result = 0;
        try
        {
            ThrowIf(exception);
            result |= 1;
        }
        catch (Exception ex) when (ex is ArithmeticException or ArgumentException)
        {
            result |= 2;
        }
        catch (InvalidOperationException)
        {
            result |= 4;
        }
        catch (SystemException ex) when (ex is FormatException || ex.Message == "foo")
        {
            result |= 8;
        }
        finally
        {
            result |= 0x10;
        }
        return result;
    }

    public int TryCatchNotSupportedException(Exception? exception)
    {
        int result = 0;
        try
        {
            ThrowIf(exception);
            result |= 1;
        }
        catch (Exception ex) when (ex is ArithmeticException or ArgumentException)
        {
            result |= 2;
        }
        catch (InvalidOperationException)
        {
            result |= 4;
        }
        catch (SystemException ex) when (ex is FormatException || ex.Message == "foo")
        {
            result |= 8;
        }
        finally
        {
            result |= 0x10;
        }
        return result;
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void ThrowIf(MyBool condition)
    {
        if (condition) throw new InvalidOperationException();
    }

    [ExcludeFromCodeCoverage]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void ThrowIf(Exception? exception)
    {
        if (exception is not null) throw exception;
    }
}
