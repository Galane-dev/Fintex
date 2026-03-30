import json
import sys


def respond(success, error=None, account=None):
    payload = {
        "success": success,
        "error": error,
        "account": account,
    }
    sys.stdout.write(json.dumps(payload))
    sys.stdout.flush()


def main():
    try:
        payload = json.loads(sys.stdin.read() or "{}")
    except Exception as exception:
        respond(False, f"Bridge input was invalid: {exception}")
        return

    try:
        import MetaTrader5 as mt5
    except Exception as exception:
        respond(False, f"The MetaTrader5 Python package is not available: {exception}")
        return

    initialize_kwargs = {}
    terminal_path = payload.get("terminalPath")
    if terminal_path:
        initialize_kwargs["path"] = terminal_path

    initialized = False
    try:
        initialized = mt5.initialize(**initialize_kwargs) if initialize_kwargs else mt5.initialize()
        if not initialized:
            respond(False, f"MetaTrader 5 could not initialize: {mt5.last_error()}")
            return

        login_value = str(payload.get("login") or "").strip()
        password = payload.get("password") or ""
        server = payload.get("server") or ""

        if not login_value or not password or not server:
            respond(False, "Login, password, and server are required for MetaTrader validation.")
            return

        logged_in = mt5.login(login=int(login_value), password=password, server=server)
        if not logged_in:
            respond(False, f"MetaTrader login failed: {mt5.last_error()}")
            return

        account_info = mt5.account_info()
        if account_info is None:
            respond(False, "MetaTrader login succeeded but account information could not be loaded.")
            return

        respond(
            True,
            account={
                "login": str(getattr(account_info, "login", login_value)),
                "name": getattr(account_info, "name", None),
                "server": getattr(account_info, "server", server),
                "company": getattr(account_info, "company", None),
                "currency": getattr(account_info, "currency", None),
                "leverage": getattr(account_info, "leverage", None),
                "balance": getattr(account_info, "balance", None),
                "equity": getattr(account_info, "equity", None),
            },
        )
    except Exception as exception:
        respond(False, f"MetaTrader bridge exception: {exception}")
    finally:
        if initialized:
            try:
                mt5.shutdown()
            except Exception:
                pass


if __name__ == "__main__":
    main()
