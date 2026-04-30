import os
import argparse
import shutil
import zipfile


def vnd_packbin(extra_args):
    solution = os.path.dirname(os.path.abspath(__file__ + '/../'))

    # Get extra arguments to parse for release config
    parser = argparse.ArgumentParser(prog='adt packbin',
                                     add_help=False)
    parser.add_argument('-c', '--config', default='Release')
    parsed = parser.parse_args(extra_args)

    # Get the project version
    build_props = solution + '/Directory.Build.props'
    version = ""
    with open(build_props) as props_file:
        lines = props_file.readlines()
        version_found = False
        for line in lines:
            if '<Version>' in line and '</Version>' in line:
                version_found = True
                substr_idx = line.index('<') + 9
                substr_idx_end = line[substr_idx:].index('<') + substr_idx
                version = line[substr_idx:substr_idx_end]
                break
        if not version_found:
            raise Exception("Version not found in Directory.Build.props.")
    
    # Make a zip archive file path
    exec_dir = solution + f'/private/BassBoom.Cli/bin/{config}/net10.0/'
    exec_dnfx_dir = solution + f'/private/BassBoom.Cli/bin/{config}/net48/'
    artifacts_dir = solution + '/artifacts'
    exec_zip_file = f'{version}-bin'
    exec_zip_path = artifacts_dir + '/' + exec_zip_file
    exec_dnfx_zip_file = f'{version}-bin-48'
    exec_dnfx_zip_path = artifacts_dir + '/' + exec_dnfx_zip_file

    # Generate the files
    zip_path = shutil.make_archive(exec_zip_path, 'zip', exec_dir)
    print(f"Written to {zip_path}")
    zip_path = shutil.make_archive(exec_dnfx_zip_path, 'zip', exec_dnfx_dir)
    print(f"Written to {zip_path}")
    
    # Do the same for QuickPlay
    exec_dir = solution + f'/private/BassBoom.QuickPlay/bin/{config}/net10.0/'
    exec_dnfx_dir = solution + f'/private/BassBoom.QuickPlay/bin/{config}/net48/'
    artifacts_dir = solution + '/artifacts'
    exec_zip_file = f'{version}-quickplay-bin'
    exec_zip_path = artifacts_dir + '/' + exec_zip_file
    exec_dnfx_zip_file = f'{version}-quickplay-bin-48'
    exec_dnfx_zip_path = artifacts_dir + '/' + exec_dnfx_zip_file
    zip_path = shutil.make_archive(exec_zip_path, 'zip', exec_dir)
    print(f"Written to {zip_path}")
    zip_path = shutil.make_archive(exec_dnfx_zip_path, 'zip', exec_dnfx_dir)
    print(f"Written to {zip_path}")
    
    # Do the same for QuickRadio
    exec_dir = solution + f'/private/BassBoom.QuickRadio/bin/{config}/net10.0/'
    exec_dnfx_dir = solution + f'/private/BassBoom.QuickRadio/bin/{config}/net48/'
    artifacts_dir = solution + '/artifacts'
    exec_zip_file = f'{version}-quickradio-bin'
    exec_zip_path = artifacts_dir + '/' + exec_zip_file
    exec_dnfx_zip_file = f'{version}-quickradio-bin-48'
    exec_dnfx_zip_path = artifacts_dir + '/' + exec_dnfx_zip_file
    zip_path = shutil.make_archive(exec_zip_path, 'zip', exec_dir)
    print(f"Written to {zip_path}")
    zip_path = shutil.make_archive(exec_dnfx_zip_path, 'zip', exec_dnfx_dir)
    print(f"Written to {zip_path}")
