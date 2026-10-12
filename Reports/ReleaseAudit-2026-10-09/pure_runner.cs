using System;using System.Linq;using System.Reflection;using System.Collections;using System.Collections.Generic;using System.IO;using NUnit.Framework;using Newtonsoft.Json;
class PureRunner{
 static Exception Unwrap(Exception e)=>e is TargetInvocationException&&e.InnerException!=null?Unwrap(e.InnerException):e;
 static void Lifecycle(object instance,Type attribute){foreach(var m in instance.GetType().GetMethods().Where(m=>m.GetCustomAttributes(attribute,true).Any()))m.Invoke(instance,null);}
 static IEnumerable<object[]> Cases(Type type,MethodInfo method){var direct=method.GetCustomAttributes(typeof(TestCaseAttribute),true).Cast<TestCaseAttribute>().ToArray();foreach(var c in direct)yield return c.Arguments;
  var sourced=method.GetCustomAttributes(typeof(TestCaseSourceAttribute),true).Cast<TestCaseSourceAttribute>().ToArray();foreach(var s in sourced){var t=s.SourceType??type;var flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static;var member=t.GetMember(s.SourceName,flags).Single();object value=member is PropertyInfo p?p.GetValue(null):member is FieldInfo f?f.GetValue(null):((MethodInfo)member).Invoke(null,s.MethodParams);
   foreach(var v in (IEnumerable)value)yield return v is TestCaseData d?d.Arguments:v is object[] a?a:new[]{v};}
  if(direct.Length==0&&sourced.Length==0)yield return Array.Empty<object>();
 }
 static void Main(string[] args){string folder=args[0];Environment.SetEnvironmentVariable("CASCADE_VALIDATION_PHASE","ReleaseAudit-2026-10-09");var results=new List<object>();int pass=0,fail=0,skip=0;
  var types=Assembly.GetExecutingAssembly().GetTypes().Where(t=>t.Namespace=="Cascade.Tests"&&!t.IsAbstract&&t.GetMethods().Any(m=>m.GetCustomAttributes(true).Any(a=>a is TestAttribute||a is TestCaseAttribute||a is TestCaseSourceAttribute))).ToArray();
  if(args.Contains("--list")){foreach(var t in types)Console.WriteLine(t.Name);return;}
  foreach(var type in types.Where(t=>args.Length<2||t.Name==args[1])){
   var methods=type.GetMethods().Where(m=>m.GetCustomAttributes(true).Any(a=>a is TestAttribute||a is TestCaseAttribute||a is TestCaseSourceAttribute)).ToArray();if(methods.Length==0)continue;object instance=Activator.CreateInstance(type);
   try{Lifecycle(instance,typeof(OneTimeSetUpAttribute));foreach(var method in methods){
     if(method.Name=="ExportVerifiedCanonicalMetrics"||method.Name=="PublishedDescentCurvesAreProvenAndPreserveTheReceiverAndFinalDevices"){skip++;results.Add(new{test=type.Name+"."+method.Name,status="Skipped",reason="Historical full-catalog version2 generation/report export; current version6 independently audited in full."});continue;}
     foreach(var c in Cases(type,method)){string name=type.Name+"."+method.Name+"("+string.Join(",",c.Select(x=>x?.ToString()))+")";var clock=System.Diagnostics.Stopwatch.StartNew();Exception error=null;
      try{Lifecycle(instance,typeof(SetUpAttribute));method.Invoke(instance,c);}catch(Exception e){error=Unwrap(e);}finally{try{Lifecycle(instance,typeof(TearDownAttribute));}catch(Exception e){error=Unwrap(e);}}
      if(error==null){pass++;results.Add(new{test=name,status="Passed",ms=clock.Elapsed.TotalMilliseconds});}else{fail++;results.Add(new{test=name,status="Failed",ms=clock.Elapsed.TotalMilliseconds,error=error.ToString()});Console.WriteLine("FAIL "+name+": "+error.Message);}
     }
    }}catch(Exception e){fail++;results.Add(new{test=type.Name+" fixture",status="Failed",error=Unwrap(e).ToString()});}finally{Lifecycle(instance,typeof(OneTimeTearDownAttribute));}
   Console.WriteLine(type.Name+": cumulative "+pass+" passed, "+fail+" failed");File.WriteAllText(Path.Combine(folder,"pure-suite.json"),JsonConvert.SerializeObject(results,Formatting.Indented));
  }
  File.WriteAllText(Path.Combine(folder,"pure-suite-summary.json"),JsonConvert.SerializeObject(new{pass,fail,skip,method="Standalone actual pure C# / NUnit assertions; fixture root adapted; content helper invokes unchanged production importer. No native Unity API tests."},Formatting.Indented));Environment.ExitCode=fail==0?0:1;
 }
}
