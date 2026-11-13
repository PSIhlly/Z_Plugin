"""Compile and execute Copy methods emitted by the real Excel2Cs generator."""
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'Z_OtherProjects/Z_Tool/Excel2Cs'))
import Excel2Cs
from formBaseInfo import FormInfo


class GeneratedCopyTests(unittest.TestCase):
    def test_generated_graphs(self):
        declarations = []
        for name, fields in (
            ('A', {'id': 'int', 'peer': 'BForm.Data', 'rows': 'List<Dictionary<string,BForm.Data>>',
                   'numbers': 'List<List<int>>', 'pair': '(int,BForm.Data)'}),
            ('B', {'id': 'int', 'peer': 'AForm.Data', 'value': 'int'}),
        ):
            form = FormInfo('', 'Test')
            form.name = name
            form.var_type_dic = fields
            form.var_config_dic = {key: [] for key in fields}
            Excel2Cs.form_info_list = [form]
            Excel2Cs.create_data_handle()
            members = '\n'.join(f'public {kind} {key};' for key, kind in fields.items())
            declarations.append(f'public static class {name}Form {{ public static Chain idChain = new Chain(); '
                                + f'public partial class Data {{ {members} {form.declare_str} }} }}')
        source = '''using System;
using System.Collections.Generic;
using Z_DesignStyle;
public class Chain { public int GetId() => 99; }
''' + '\n'.join(declarations) + '''
public static class Program {
 static int checks;
 static void Check(bool value) { if(!value) throw new Exception("check " + checks); checks++; }
 static void Cycle(Action copy) {
  try { copy(); } catch(InvalidOperationException e) {
   Check(e.Message.Contains("Copy() circular object reference") && e.Message.Contains(" -> "));
   return;
  }
  throw new Exception("missing cycle exception");
 }
 public static void Main() {
  var b = new BForm.Data(2, null, 7);
  var a = new AForm.Data(1, b,
   new List<Dictionary<string,BForm.Data>> {new Dictionary<string,BForm.Data>{{"same",b},{"null",null}}, null},
   new List<List<int>> {new List<int>{1,2},null}, (8,b));
  var copy=a.Copy();
  Check(copy.id==1 && copy.peer.id==2 && copy.peer.value==7);
  Check(!Object.ReferenceEquals(copy.peer,b));
  Check(!Object.ReferenceEquals(copy.rows[0]["same"],b));
  Check(!Object.ReferenceEquals(copy.rows[0]["same"],copy.peer));
  Check(copy.rows[0]["null"]==null && copy.rows[1]==null && copy.numbers[1]==null);
  copy.numbers[0][0]=42; Check(a.numbers[0][0]==1);
  Check(!Object.ReferenceEquals(copy.pair.Item2,b) && copy.pair.Item1==8);
  Check(a.Copy(false).id==99 && a.Copy(false).peer.id==2);
  b.peer=a; Cycle(()=>a.Copy()); Cycle(()=>b.Copy());
  b.peer=null; Check(a.Copy().peer.value==7);
  a.peer=null; a.rows=null; a.numbers=null;
  Check(a.Copy().peer==null && a.Copy().rows.Count==0 && a.Copy().numbers.Count==0);
  Console.WriteLine("EXCEL_COPY_GENERATION_PASS checks="+checks);
 }
}
'''
        with tempfile.TemporaryDirectory(prefix='ExcelCopy-', dir=ROOT / 'Temp') as directory:
            directory = Path(directory)
            (directory / 'Program.cs').write_text(source, encoding='utf-8')
            guard = (ROOT / 'Assets/Z_Level0/Z_DesignStyle/Core/DataCopyGuard.cs').read_text(encoding='utf-8')
            (directory / 'DataCopyGuard.cs').write_text(guard, encoding='utf-8')
            (directory / 'Test.csproj').write_text(
                '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType>'
                '<TargetFramework>net9.0</TargetFramework></PropertyGroup></Project>', encoding='utf-8')
            result = subprocess.run(['dotnet', 'run', '--project', str(directory / 'Test.csproj'), '-v:q'],
                                    capture_output=True, text=True)
            self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
            self.assertIn('EXCEL_COPY_GENERATION_PASS', result.stdout)
            print(result.stdout.strip())


if __name__ == '__main__':
    unittest.main()
