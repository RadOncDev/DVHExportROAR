#!/bin/bash

# Check if filename and number of files are provided
if [ $# -lt 2 ]; then
    echo "Usage: $0 <filename> <number_of_files>"
    exit 1
fi



filename_org="$1"
filename="${filename_org%.*}"

number_of_files=$2
total_lines=$(wc -l < "$filename_org")
lines_per_file=$((total_lines / number_of_files))
current_file=1
current_line=0
prev_key=""

# Read file line by line
while IFS=$'\t' read -r key rest; do
    if [ $current_file -le $number_of_files ]; then
        # Start a new file if lines exceed lines_per_file and first column value changes
        if [ $current_line -ge $lines_per_file ] && [ "$key" != "$prev_key" ]; then
            current_file=$((current_file + 1))
            current_line=0
        fi
        echo -e "$key\t$rest" >> "${filename}_part${current_file}.txt"
        current_line=$((current_line + 1))
    else
        # Append to the last file if the number of files exceeds the limit
        echo -e "$key\t$rest" >> "${filename}_part${number_of_files}.txt"
    fi
    prev_key=$key
done < "$filename_org"



for i in $(seq 1 $number_of_files)
do
    ../DVHExportROAR_v1.2.0.exe ./"${filename}_part${i}.txt" > "${filename}_part${i}_output.txt" 2> "${filename}_part${i}_log.txt" &
done

