// Copyright (c) Matthias Wolf, Mawosoft.

using System.Xml.Linq;

namespace ReportComparer;

internal static partial class ReportParser
{
    private static readonly XName s_block_coverage = "block_coverage";
    private static readonly XName s_blocks_covered = "blocks_covered";
    private static readonly XName s_blocks_not_covered = "blocks_not_covered";
    //private static readonly XName s_branch = "branch";
    private static readonly XName s_branch_rate = "branch-rate";
    private static readonly XName s_branches_covered = "branches-covered";
    private static readonly XName s_branches_valid = "branches-valid";
    private static readonly XName s_class = "class";
    private static readonly XName s_classes = "classes";
    private static readonly XName s_complexity = "complexity";
    private static readonly XName s_condition = "condition";
    private static readonly XName s_condition_coverage = "condition-coverage";
    private static readonly XName s_conditions = "conditions";
    private static readonly XName s_coverage = "coverage";
    private static readonly XName s_covered = "covered";
    private static readonly XName s_end_column = "end_column";
    private static readonly XName s_end_line = "end_line";
    private static readonly XName s_filename = "filename";
    private static readonly XName s_function = "function";
    private static readonly XName s_functions = "functions";
    private static readonly XName s_hits = "hits";
    private static readonly XName s_id = "id";
    private static readonly XName s_line = "line";
    private static readonly XName s_line_coverage = "line_coverage";
    private static readonly XName s_line_rate = "line-rate";
    private static readonly XName s_lines = "lines";
    private static readonly XName s_lines_coveredCobertura = "lines-covered";
    private static readonly XName s_lines_coveredDynamicCoverage = "lines_covered";
    private static readonly XName s_lines_not_covered = "lines_not_covered";
    private static readonly XName s_lines_partially_covered = "lines_partially_covered";
    private static readonly XName s_lines_valid = "lines-valid";
    private static readonly XName s_method = "method";
    private static readonly XName s_methods = "methods";
    private static readonly XName s_module = "module";
    private static readonly XName s_modules = "modules";
    private static readonly XName s_name = "name";
    private static readonly XName s_namespace = "namespace";
    private static readonly XName s_number = "number";
    private static readonly XName s_package = "package";
    private static readonly XName s_packages = "packages";
    private static readonly XName s_path = "path";
    private static readonly XName s_range = "range";
    private static readonly XName s_ranges = "ranges";
    //private static readonly XName s_reason = "reason";
    private static readonly XName s_results = "results";
    private static readonly XName s_signature = "signature";
    private static readonly XName s_skipped_function = "skipped_function";
    private static readonly XName s_skipped_functions = "skipped_functions";
    private static readonly XName s_skipped_module = "skipped_module";
    private static readonly XName s_skipped_modules = "skipped_modules";
    private static readonly XName s_source = "source";
    private static readonly XName s_source_file = "source_file";
    private static readonly XName s_source_files = "source_files";
    private static readonly XName s_source_id = "source_id";
    private static readonly XName s_sources = "sources";
    private static readonly XName s_start_column = "start_column";
    private static readonly XName s_start_line = "start_line";
    //private static readonly XName s_type = "type";
    private static readonly XName s_type_name = "type_name";
}
