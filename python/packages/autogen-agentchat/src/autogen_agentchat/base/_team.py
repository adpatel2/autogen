from typing import Any, Mapping
from abc import ABC, abstractmethod
from ._task import TaskRunner
from pydantic import BaseModel
from autogen_core import ComponentBase


class Team(ABC, TaskRunner, ComponentBase[BaseModel]):
    component_type = "team"

    @abstractmethod
    async def reset(self) -> None:
        """Reset the team and all its participants to its initial state."""
        ...

    @abstractmethod
    async def save_state(self) -> Mapping[str, Any]:
        """Save the current state of the team."""
        ...

    @abstractmethod
    async def load_state(self, state: Mapping[str, Any]) -> None:
        """Load the state of the team."""
        ...
