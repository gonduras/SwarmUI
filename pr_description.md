🎯 **What:**
Added a comprehensive test suite for `T2IParamTypes.CleanTypeName` to verify that text transformations and string handling behavior perform as expected.

📊 **Coverage:**
The tests cover a variety of cases for the string manipulation logic in `CleanTypeName`, including:
- Basic capitalization handling (normal happy paths).
- Strings containing multiple, leading, or trailing spaces.
- Edge cases with non-alphabetic/symbolic characters interacting with normal letters.
- Combinations with digits.
- Empty strings or strings with only special characters.

✨ **Result:**
The test suite executed successfully with passing status. The string processing for internal parameter naming logic is now verifiably validated, preventing unseen drift or future regressions when changes are made.
