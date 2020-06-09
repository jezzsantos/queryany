using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Task = System.Threading.Tasks.Task;

/// <summary>
/// Hook interface for all assertion extension methods we might ever need :).
/// </summary>
public interface IAssertion
{
}

/// <summary>
/// Basic implementation of an assertion, which provides some helper methods that extensions
/// can use.
/// </summary>
[DebuggerStepThrough]
[DebuggerNonUserCode]
public class Assertion : IAssertion
{
    /// <summary>
    /// If a custom message is provided, it's formatted with the given arguments (if any)
    /// and prepended to the fixed message, with a new-line separating them.
    /// </summary>
    public static string FormatMessage(string fixedMessage, string customMessage, params object[] args)
    {
        if (!string.IsNullOrEmpty(customMessage))
        {
            return fixedMessage;
        }

        return string.Format(customMessage, args) + Environment.NewLine + fixedMessage;
    }
}

[DebuggerStepThrough]
[DebuggerNonUserCode]
public static class BasicAssertions
{
    public static void Contains<T>(this IAssertion assertion, T expected, IEnumerable<T> collection)
    {
        CollectionAssert.Contains(new List<T>(collection), expected);
    }

    public static void OfType<T>(this IAssertion assertion, T value, Type expectedType)
    {
        Assert.IsInstanceOfType(value, expectedType);
    }

    public static void OfType<T>(this IAssertion assertion, T value, Type expectedType, string message,
        params object[] args)
    {
        Assert.IsInstanceOfType(value, expectedType, EscapeMessage(message), args);
    }

    public static void Contains<T>(this IAssertion assertion, T expected, IEnumerable<T> collection, string message,
        params object[] args)
    {
        CollectionAssert.Contains(new List<T>(collection), expected, EscapeMessage(message), args);
    }

    public static void Equal<T>(this IAssertion assertion, T expected, T actual)
    {
        Assert.AreEqual(expected, actual);
    }

    public static void Near(this IAssertion assertion, int expected, int actual, int within = 1)
    {
        Assert.IsTrue((expected + within) >= actual
                      && (expected - within) <= actual,
            @"Expected number ({0}) to be within {1} of actual number ({2})",
            expected, within, actual);
    }

    public static void Near(this IAssertion assertion, DateTime expected, DateTime actual, int withinMilliseconds = 500)
    {
        assertion.Near(expected, actual, TimeSpan.FromMilliseconds(withinMilliseconds));
    }

    public static void Near(this IAssertion assertion, DateTime expected, DateTime actual, TimeSpan within)
    {
        var withinMilliseconds = within.TotalMilliseconds;
        Assert.IsTrue(expected.AddMilliseconds(withinMilliseconds) >= actual
                      && expected.AddMilliseconds(-withinMilliseconds) <= actual,
            @"Expected date ({0:O}) to be within {1}ms of actual date ({2:O})",
            expected, withinMilliseconds, actual);
    }

    public static void Equal<T>(this IAssertion assertion, T expected, T actual, string message, params object[] args)
    {
        Assert.AreEqual(expected, actual, EscapeMessage(message), args);
    }

    public static void NotEqual<T>(this IAssertion assertion, T expected, T actual)
    {
        Assert.AreNotEqual(expected, actual);
    }

    public static void NotEqual<T>(this IAssertion assertion, T expected, T actual, string message, params object[] args)
    {
        Assert.AreNotEqual(expected, actual, EscapeMessage(message), args);
    }

    public static void Null(this IAssertion assertion, object @object)
    {
        Assert.IsNull(@object);
    }

    public static void Null(this IAssertion assertion, object @object, string message, params object[] args)
    {
        Assert.IsNull(@object, EscapeMessage(message), args);
    }

    public static void NotNull(this IAssertion assertion, object @object)
    {
        Assert.IsNotNull(@object);
    }

    public static void NotNull(this IAssertion assertion, object @object, string message, params object[] args)
    {
        Assert.IsNotNull(@object, EscapeMessage(message), args);
    }

    public static void Same(this IAssertion assertion, object expected, object actual)
    {
        Assert.AreSame(expected, actual);
    }

    public static void Same(this IAssertion assertion, object expected, object actual, string message,
        params object[] args)
    {
        Assert.AreSame(expected, actual, EscapeMessage(message), args);
    }

    public static void NotSame(this IAssertion assertion, object notExpected, object actual)
    {
        Assert.AreNotSame(notExpected, actual);
    }

    public static void NotSame(this IAssertion assertion, object notExpected, object actual, string message,
        params object[] args)
    {
        Assert.AreNotSame(notExpected, actual, EscapeMessage(message), args);
    }

    public static void Fail(this IAssertion assertion)
    {
        Assert.Fail();
    }

    public static void Fail(this IAssertion assertion, string message, params object[] args)
    {
        Assert.Fail(EscapeMessage(message), args);
    }

    private static string EscapeMessage(string message)
    {
        return message.Replace("{", "{{").Replace("}", "}}");
    }

    public static void True(this IAssertion assertion, bool condition)
    {
        Assert.IsTrue(condition);
    }

    public static void True(this IAssertion assertion, bool condition, string message, params object[] args)
    {
        Assert.IsTrue(condition, EscapeMessage(message), args);
    }

    public static void False(this IAssertion assertion, bool condition)
    {
        Assert.IsFalse(condition);
    }

    public static void False(this IAssertion assertion, bool condition, string message, params object[] args)
    {
        Assert.IsFalse(condition, EscapeMessage(message), args);
    }

    public static void Inconclusive(this IAssertion assertion)
    {
        Assert.Inconclusive();
    }

    public static void Inconclusive(this IAssertion assertion, string message, params object[] parameters)
    {
        Assert.Inconclusive(EscapeMessage(message), parameters);
    }

    public static Exception Throws<TException>(this IAssertion assertion, string exceptionContains, Action action)
        where TException : Exception
    {
        return Throws<TException>(assertion, exceptionContains, action, null);
    }

    public static Exception Throws<TException>(this IAssertion assertion, string exceptionContains, Action action, string message,
        params object[] args)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            if (!(ex is TException))
            {
                throw BuildThrowsException(ex, Assertion.FormatMessage(
                    $"expected exception of type {typeof(TException)} but was {ex.GetType()}.",
                    message, args));
            }

            if (!IsFormattedFrom(ex.Message, exceptionContains))
            {
                throw BuildThrowsException(ex, Assertion.FormatMessage(
                    $"throw exception did not match message regex match pattern '{exceptionContains}'.",
                    message, args));
            }

            return ex;
        }

        throw new AssertFailedException(
            Assertion.FormatMessage("Assert.Throws() was expecting an exception, but none was thrown", message, args));
    }

    public static TException Throws<TException>(this IAssertion assertion, Action action)
        where TException : Exception
    {
        return Throws<TException>(assertion, action, null);
    }

    public static TException Throws<TException>(this IAssertion assertion, Action action, string exceptionContains, params object[] args)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            if (!(ex is TException))
            {
                throw BuildThrowsException(ex,
                    $"expected exception of type {typeof(TException)} but was {ex.GetType()}.");
            }

            return (TException)ex;
        }

        throw new AssertFailedException(Assertion.FormatMessage(
            "Assert.Throws() did not throw any exception. Expected " + typeof(TException).Name + ".", exceptionContains, args));
    }

    public static async Task<TException> ThrowsAsync<TException>(this IAssertion assertion, Func<Task> func) where TException : Exception
    {
        try
        {
            await func();
            assertion.Throws<TException>(() =>
            {
            });
        }
        catch (TException exception)
        {
            return exception;
        }

        return null;
    }

    public static async Task<TException> ThrowsAsync<TException>(this IAssertion assertion, string exceptionContains, Func<Task> func) where TException : Exception
    {
        try
        {
            await func();
            assertion.Throws<TException>(exceptionContains, () =>
            {
            });
        }
        catch (TException exception)
        {
            return exception;
        }

        return null;
    }

    private static bool IsFormattedFrom(string formattedString, string formatString)
    {
        var escapedPattern = formatString
            .Replace("[", "\\[")
            .Replace("]", "\\]")
            .Replace("(", "\\(")
            .Replace(")", "\\)")
            .Replace(".", "\\.")
            .Replace("<", "\\<")
            .Replace(">", "\\>");

        var pattern = Regex.Replace(escapedPattern, @"\{\d+\}", ".*")
            .Replace(" ", @"\s");

        return new Regex(pattern).IsMatch(formattedString);
    }


    #region StackTrace Hacks

    internal static Exception BuildThrowsException(Exception ex, string message)
    {
        PreserveStackTrace(ex);
        Exception result = new AssertFailedException("Assert.Throws() failure: " + message + "\r\n" + ex.Message);

        var remoteStackTraceString = typeof(Exception)
            .GetField("_remoteStackTraceString",
                BindingFlags.Instance | BindingFlags.NonPublic);

        var currentMethod = MethodBase.GetCurrentMethod().Name;
        var stackTrace = string.Join(Environment.NewLine,
            ex.StackTrace
                .Split(new[]
                {
                    Environment.NewLine
                }, StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .Where(frame => !frame.Contains(currentMethod))
                .ToArray());

        remoteStackTraceString?.SetValue(
            result,
            stackTrace + Environment.NewLine);

        // Roundtrip via serialization to cause the full exception data to be persisted, 
        // and to cause the remote stacktrace to be cleaned-up.
        var selector = new ExceptionSurrogateSelector();
        var formatter = new BinaryFormatter(selector, new StreamingContext(StreamingContextStates.All));
        using (var mem = new MemoryStream())
        {
            formatter.Serialize(mem, result);
            mem.Position = 0;
            result = (Exception)formatter.Deserialize(mem);
            PreserveStackTrace(result);
        }

        return result;
    }

    private static void PreserveStackTrace(Exception exception)
    {
        var preserveStackTrace = typeof(Exception).GetMethod("InternalPreserveStackTrace",
            BindingFlags.Instance | BindingFlags.NonPublic);
        preserveStackTrace?.Invoke(exception, null);
    }

    private class ExceptionSurrogateSelector : ISurrogateSelector
    {
        private ISurrogateSelector selector;

        // ReSharper disable once ParameterHidesMember
        public void ChainSelector(ISurrogateSelector selector)
        {
            this.selector = selector;
        }

        public ISurrogateSelector GetNextSelector()
        {
            return this.selector;
        }

        // ReSharper disable once ParameterHidesMember
        public ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector)
        {
            if (typeof(Exception).IsAssignableFrom(type))
            {
                selector = this;

                return new ExceptionSurrogate();
            }

            selector = null;
            return null;
        }
    }

    private class ExceptionSurrogate : ISerializationSurrogate
    {
        private static readonly MethodInfo updateMethod = typeof(SerializationInfo).GetMethod("UpdateValue",
            BindingFlags.Instance |
            BindingFlags.NonPublic |
            BindingFlags.InvokeMethod);

        private static readonly Action<SerializationInfo, string, object, Type> updateValue;

        static ExceptionSurrogate()
        {
            updateValue = (info, name, value, type) => updateMethod.Invoke(info, new[]
            {
                name,
                value,
                type
            });
        }

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            ((ISerializable)obj).GetObjectData(info, context);

            // Skip our own frames to cleanup the trace.
            var stackTrace = string.Join(Environment.NewLine,
                info.GetString("RemoteStackTraceString")
                    .Split(new[]
                        {
                            Environment.NewLine
                        },
                        StringSplitOptions.RemoveEmptyEntries)
                    .Where(frame => !frame.Contains("Assertions"))
                    .ToArray());

            updateValue(info, "RemoteStackTraceString", stackTrace, typeof(string));
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context,
            ISurrogateSelector selector)
        {
            var serializationCtor = obj.GetType().GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[]
                {
                    typeof(SerializationInfo),
                    typeof(StreamingContext)
                },
                null);

            Debug.Assert(serializationCtor != null, "Serialization constructor not found.", "Object type {0}.",
                obj.GetType());

            serializationCtor?.Invoke(obj, new object[]
            {
                info,
                context
            });

            return obj;
        }
    }

    #endregion
}

public static class AssertionExtensions
{
    /// <summary>
    /// Asserts that a <see cref="WebException" /> was thrown with the specified <see cref="HttpStatusCode" /> and specified
    /// <see cref="WebException.Status" />
    /// </summary>
    public static void ThrowsWebException(this IAssertion assertion, HttpStatusCode statusCode, Action action)
    {
        ThrowsWebException(assertion, statusCode, null, action);
    }

    /// <summary>
    /// Asserts that a <see cref="WebException" /> was thrown with the specified <see cref="HttpStatusCode" /> and specified
    /// <see cref="WebException.Status" />
    /// </summary>
    public static void ThrowsWebException(this IAssertion assertion, HttpStatusCode statusCode, string errorMessage, Action action)
    {
        try
        {
            action();

            assertion.Fail($"Expected 'WebException' (with statusCode: '{statusCode}') to be thrown, but it was not.");
        }
        catch (WebException ex)
        {
            assertion.Equal(statusCode, GetStatus(ex),
                $"Expected 'WebException' with statusCode: '{statusCode}' to be thrown, but was '{GetStatus(ex)}': {ex.Message}");
            if (string.IsNullOrEmpty(errorMessage))
            {
                assertion.Equal(errorMessage, ex.Message,
                    $"Expected 'WebException' (with statusCode: '{GetStatus(ex)}') to have message '{errorMessage}', but was '{ex.Message}'");
            }
        }
        catch (Exception ex)
        {
            assertion.Fail($"Expected 'WebException' (with statusCode: '{statusCode}') to be thrown, but exception {ex.GetType()}: {ex.Message} was thrown instead");
        }
    }

    private static HttpStatusCode? GetStatus(WebException ex)
    {
        return (ex?.Response as HttpWebResponse)?.StatusCode;
    }
}
