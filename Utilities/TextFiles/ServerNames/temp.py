from pathlib import Path

# Load and process the file
file_path = Path("Person.txt")
lines = file_path.read_text().splitlines()

# Remove the first line if it's just a number count
if lines[0].isdigit():
    lines = lines[1:]

# Use a set to track unique prefix+suffix combinations
seen = set()
unique_entries = []

for line in lines:
    parts = line.split('-')
    if len(parts) >= 3:
        prefix = parts[0]
        suffix = parts[-1]
        key = f"{prefix}-{suffix}"
        if key not in seen:
            seen.add(key)
            unique_entries.append(line)

# Save the filtered unique entries to a new file
output_path = Path("Filtered_Unique_Hostnames.txt")
output_path.write_text('\n'.join(unique_entries))

output_path