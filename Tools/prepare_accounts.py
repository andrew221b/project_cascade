#!/usr/bin/env python3
"""Check, prepare or explicitly deploy CASCADE accounts. Never enables production automatically."""
import argparse
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys
import tempfile
import uuid
from urllib.parse import urlparse

ROOT = Path(__file__).resolve().parents[1]
CONFIG = ROOT / "Configs/authentication.json"
PROJECT = "a1b8a760-5a0c-4c2d-8ae7-54c1b411259a"
REPORT = ROOT / "Reports/AccountActivation-Oct10"
MODULE = ROOT / "CascadeUnity/CascadeAccountCCM/CascadeAccount/CascadeAccount.csproj"
CLI_ASSET = ROOT / "CascadeUnity/CascadeAccountCCM/CascadeAccount.sln"

def configuration():
    def unique(pairs):
        result = {}
        for key, value in pairs:
            if key in result:
                raise ValueError("Duplicate field in authentication.json.")
            result[key] = value
        return result
    if CONFIG.stat().st_size > 16384:
        raise ValueError("Authentication configuration is too large.")
    config = json.loads(CONFIG.read_text(), object_pairs_hook=unique)
    strings = ("unityProjectId", "unityPlayerAccountsClientId", "unityEnvironmentName", "cloudCodeModuleName")
    flags = ("cloudCodeDeployedAndVerified", "purchaseVerificationConfigured")
    optional = {"serverRewardsConfigured", "accountDeletionUrl"}
    if not set(strings + flags).issubset(config) or set(config) - set(strings + flags) - optional or any(type(config[k]) is not str for k in strings) or any(type(config[k]) is not bool for k in flags):
        raise ValueError("authentication.json has missing, unknown or incorrectly typed fields.")
    if str(uuid.UUID(config["unityProjectId"])) != PROJECT or config["unityProjectId"] != PROJECT:
        raise ValueError("Cloud Project ID does not match this project.")
    for key in strings[2:]:
        if not re.fullmatch(r"[A-Za-z0-9_-]{1,64}", config[key]):
            raise ValueError(f"Invalid {key}.")
    client = config["unityPlayerAccountsClientId"]
    if client and not re.fullmatch(r"[A-Za-z0-9_.-]{1,128}", client):
        raise ValueError("Invalid public Player Accounts Client ID.")
    if config["cloudCodeDeployedAndVerified"] and not client:
        raise ValueError("A client ID is required before cloud activation.")
    if config["purchaseVerificationConfigured"] and not config["cloudCodeDeployedAndVerified"]:
        raise ValueError("Purchase verification requires a verified cloud deployment.")
    if type(config.get("serverRewardsConfigured", False)) is not bool or type(config.get("accountDeletionUrl", "")) is not str:
        raise ValueError("Invalid optional account field types.")
    if config.get("serverRewardsConfigured") and not config["cloudCodeDeployedAndVerified"]:
        raise ValueError("Server rewards require verified cloud deployment.")
    if config.get("accountDeletionUrl"):
        url = urlparse(config["accountDeletionUrl"])
        if url.scheme != "https" or not url.netloc or url.username or url.query or url.fragment:
            raise ValueError("Invalid public deletion URL.")
    return config

def executable(variable, name, fallback=None):
    candidate = os.environ.get(variable) or shutil.which(name)
    if not candidate and fallback and fallback.is_file():
        candidate = str(fallback)
    if not candidate or not Path(candidate).is_file():
        raise ValueError(f"Install {name}, or set {variable} to its executable path.")
    return str(Path(candidate).resolve())

def execute(command, env, log=None):
    # Argument lists, never a shell; do not echo credential environment values.
    if log:
        with log.open("w") as output:
            result = subprocess.run(command, cwd=ROOT, env=env, stdout=output, stderr=subprocess.STDOUT)
        if result.returncode:
            raise RuntimeError(f"Command failed; inspect {log.relative_to(ROOT)}.")
    else:
        subprocess.run(command, cwd=ROOT, env=env, check=True)

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    mode = parser.add_mutually_exclusive_group()
    mode.add_argument("--check", action="store_true", help="Validate public configuration only (default).")
    mode.add_argument("--prepare", action="store_true", help="Run actual-source account/server checks and publish the module locally.")
    mode.add_argument("--dry-run", action="store_true", help="UGS deployment preview; remote credentials required; no remote mutation.")
    mode.add_argument("--deploy", action="store_true", help="Prepare and deploy only CascadeAccount to the configured environment.")
    args = parser.parse_args()
    config = configuration()
    print("Public account configuration: valid; client " + ("filled" if config["unityPlayerAccountsClientId"] else "pending"))
    print("Cloud: " + ("enabled" if config["cloudCodeDeployedAndVerified"] else "disabled until deployment/device verification"))
    if not (args.prepare or args.deploy or args.dry_run):
        print("Setup: Docs/ACCOUNT_ACTIVATION_OCT10.md")
        return
    if (args.deploy or args.dry_run) and not config["unityPlayerAccountsClientId"]:
        raise ValueError("Fill unityPlayerAccountsClientId before deployment.")
    if (args.deploy or args.dry_run) and config["cloudCodeModuleName"] != MODULE.parent.name:
        raise ValueError("The configured module name must match the prepared CascadeAccount module.")
    dotnet = executable("CASCADE_DOTNET", "dotnet", Path("/tmp/cascade-dotnet9/dotnet"))
    REPORT.mkdir(parents=True, exist_ok=True)
    env = os.environ.copy()
    env["PATH"] = str(Path(dotnet).parent) + os.pathsep + env.get("PATH", "")
    # A task-specific CLI home keeps local prep out of unrelated developer files.
    env.setdefault("DOTNET_CLI_HOME", str(Path(tempfile.gettempdir()) / "cascade-account-dotnet-home"))
    env.setdefault("DOTNET_BUNDLE_EXTRACT_BASE_DIR", str(Path(tempfile.gettempdir()) / "cascade-account-ugs-extract"))
    if args.prepare or args.deploy:
        for name, project in (("client-checks", ROOT / "Tools/AccountFlowChecks/AccountFlowChecks.csproj"),
                              ("server-checks", ROOT / "CascadeUnity/CascadeAccountCCM/CascadeAccount.Tests/CascadeAccount.Tests.csproj")):
            execute([dotnet, "run", "--project", str(project), "--configuration", "Release", "-p:UseSharedCompilation=false"], env, REPORT / (name + ".log"))
        execute([dotnet, "publish", str(MODULE), "--configuration", "Release", "--output", str(REPORT / "module"),
                 "-p:UseSharedCompilation=false"], env, REPORT / "module-publish.log")
        print("Account checks passed; deployable module prepared in Reports/AccountActivation-Oct10/module.")
    if args.deploy or args.dry_run:
        ugs = executable("CASCADE_UGS", "ugs")
        command = [ugs, "deploy", str(CLI_ASSET), "--project-id", config["unityProjectId"], "--environment-name", config["unityEnvironmentName"]]
        if args.dry_run:
            command.append("--dry-run")
        execute(command, env)
        print("Deployment " + ("preview finished." if args.dry_run else "finished. Complete live verification before setting cloudCodeDeployedAndVerified=true."))

if __name__ == "__main__":
    try:
        main()
    except (ValueError, OSError, RuntimeError, subprocess.CalledProcessError) as error:
        print(f"Account setup stopped: {error}", file=sys.stderr)
        sys.exit(1)
