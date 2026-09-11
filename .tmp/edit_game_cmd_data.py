import os
import sys

sys.path.insert(0, os.path.join(os.path.dirname(__file__), "codex_xls"))

import xlrd
from xlutils.copy import copy


workbook_path = os.path.join(
    "Assets", "GameSample", "Forms", "Excels", "Story", "Event", "GameCmdData_CmdData.xls"
)
workbook_dir = os.path.dirname(workbook_path)

if any(name.startswith("~$") and name.endswith(".xls") for name in os.listdir(workbook_dir)):
    raise RuntimeError("Excel lock file exists")

read_book = xlrd.open_workbook(workbook_path, formatting_info=True)
read_sheet = read_book.sheet_by_index(0)

if any(str(read_sheet.cell_value(row, 0)).strip() == "100108" for row in range(read_sheet.nrows)):
    raise RuntimeError("GameCmdData uid 100108 already exists")

write_book = copy(read_book)
write_sheet = write_book.get_sheet(0)
new_row = read_sheet.nrows
values = [
    100108,
    "GetCharacterName",
    "character",
    "character",
    "name",
    "string",
    "Get Character {0}'s name",
    "GetCharacterName(self)",
    10011,
    "Rpg",
    "",
]

donor_cells = write_sheet._Worksheet__rows[new_row - 1]._Row__cells
for column, value in enumerate(values):
    write_sheet.write(new_row, column, value)

target_cells = write_sheet._Worksheet__rows[new_row]._Row__cells
for column in range(len(values)):
    target_cells[column].xf_idx = donor_cells[column].xf_idx

temp_path = workbook_path + ".codex.tmp"
write_book.save(temp_path)
os.replace(temp_path, workbook_path)
print(f"Appended row {new_row}: {values}")
